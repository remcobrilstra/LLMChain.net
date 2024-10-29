using LLMChain.Core;
using LLMChain.Core.Conversations;
using LLMChain.Core.Tools;
using LLMChain.OpenAI.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMChain.OpenAI;

public class OpenAIProvider : IAIProvider
{
    private const string DefaultProvider = "OpenAI";
    public string Key {get; private set;} = DefaultProvider;
    public string DisplayName => $"{Key} - {ActiveModel}";

    public bool CanStream => true;
    public bool CanUseTools => true;

    private SystemMessage _systemPrompt { get; set; } = new SystemMessage();

    private const string API_ENDPOINT = "https://api.openai.com/v1/";

    private string ApiEndpointRoot { get; set; }

    public float Temperature { get; set; } = 0.7f;
    private string ApiKey { get; set; }

    public string ActiveModel { get; set; }


    JsonSerializerOptions jsSerializerOption = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters ={
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },

    };

    private string[] _availableModels = null;
    public string[] AvailableModels
    {
        get
        {
            if (_availableModels == null)
            {
                using (HttpClient wClient = new HttpClient())
                {
                    wClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);
                    var response = wClient.GetAsync($"{ApiEndpointRoot}models").Result;
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    var models = JsonSerializer.Deserialize<ModelListResponse>(responseContent, jsSerializerOption);
                    _availableModels = models.data.Select(x => x.id).ToArray();
                }
            }

            return _availableModels;
        }
    }




    public OpenAIProvider(string apiKey, string model = "", string apiEndpoint = API_ENDPOINT, string provider = DefaultProvider)
    {
        ApiEndpointRoot = apiEndpoint;
        Key = provider;
        ApiKey = apiKey;
        ActiveModel = model;
    }

    public async Task<Message> SendChatMessageAsync(Message message, ConversationHistory history, IEnumerable<ITool> tools = null)
    {
        return await ExecuteConversationStep(tools, history);
    }

    public async Task<Message> StreamChatMessage(Message message, ConversationHistory history, Action<string> OnStream, IEnumerable<ITool> tools = null)
    {
        return await StreamConversationStep(tools, history, OnStream);

    }


    private async Task<Message> StreamConversationStep(IEnumerable<ITool> tools, ConversationHistory history, Action<string> OnStream)
    {
        ChatCompletionRequest request = new ChatCompletionRequest
        {
            messages = ConvertToOpenAIMessages(history.GetPromptHistory().ToArray()),
            functions = ConvertInternalToOpenAIFunctions(tools),
            model = ActiveModel,
            stream = true,
            temperature = Temperature,
            stream_options = new StreamOptions() { include_usage = true }
        };
        try
        {
            using (HttpClient wClient = new HttpClient())
            {
                var requestmsg = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{ApiEndpointRoot}chat/completions"),
                    Content = new StringContent(JsonSerializer.Serialize(request, jsSerializerOption), Encoding.UTF8, "application/json"),
                };
                requestmsg.Headers.Add("Authorization", "Bearer " + ApiKey);

                var response = await wClient.SendAsync(requestmsg, HttpCompletionOption.ResponseHeadersRead);

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);



                StringBuilder strbld = new StringBuilder();

                bool functionmode = false;
                Function_Call func = new Function_Call();
                ChatCompletionResponse resp = null;
                uint inputTokenCost = 0;
                uint outputTokenCost = 0;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("data: "))
                    {
                        var json = line.Substring(6);
                        if (json == "[DONE]") break;

                        resp = JsonSerializer.Deserialize<ChatCompletionResponse>(json, jsSerializerOption);

                        if (resp.Usage != null)
                        {
                            outputTokenCost = resp.Usage.CompletionTokens;
                            inputTokenCost = resp.Usage.PromptTokens;
                        }

                        if (resp.Choices.Length == 0)
                            continue;

                        var messageDelta = resp.Choices[0].Delta;

                        if (messageDelta.function_call != null)
                        {
                            functionmode = true;
                        }

                        if (functionmode)
                        {
                            func.name += messageDelta.function_call?.name;
                            func.arguments += messageDelta.function_call?.arguments;

                            outputTokenCost = resp.Usage?.CompletionTokens ?? 0;
                            inputTokenCost = resp.Usage?.PromptTokens ?? 0;
                        }
                        else
                        {
                            try
                            {
                                if (!String.IsNullOrWhiteSpace(messageDelta.Content))
                                {
                                    OnStream(messageDelta.Content);
                                    strbld.Append(messageDelta.Content);
                                }
                            }
                            catch (JsonException)
                            {
                                // Ignore parsing errors for incomplete JSON
                            }
                        }
                    }
                }

                if (functionmode)
                {
                    await HandleFunctionCall(func, history, tools);
                    var msg = await StreamConversationStep(tools, history, OnStream);
                    //history.PushMessage(msg);
                    return msg;
                }
                else
                {
                    return new Message(strbld.ToString(), MessageType.Agent)
                    {
                        InputTokens = inputTokenCost,
                        OutputTokens = outputTokenCost
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private static Function[]? ConvertInternalToOpenAIFunctions(IEnumerable<ITool> tools)
    {
        return tools?.Select(x => new Models.Function()
        {
            name = x.Name,
            description = x.Description,
            parameters = new Parameters()
            {
                properties = x.Parameters.Properties.Select(y => new KeyValuePair<string, Models.propDescription>(y.Name, new propDescription()
                {
                    description = y.Description,
                    type = y.Type
                })).ToDictionary(),
                type = x.Parameters.ReturnType,
                required = x.Parameters.Required
            }
        }).ToArray();
    }

    private async Task<Message> ExecuteConversationStep(IEnumerable<ITool> tools, ConversationHistory history)
    {

        ChatCompletionRequest request = new ChatCompletionRequest
        {
            messages = ConvertToOpenAIMessages(history.GetPromptHistory().ToArray()),
            functions = ConvertInternalToOpenAIFunctions(tools),
            model = ActiveModel,
            stream = false,
            temperature = Temperature
        };
        try
        {
            using (HttpClient wClient = new HttpClient())
            {
                wClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);
                var response = await wClient.PostAsJsonAsync($"{ApiEndpointRoot}chat/completions", request, jsSerializerOption);

                string requestdata = await response.RequestMessage.Content.ReadAsStringAsync();
                string responsedata = await response.Content.ReadAsStringAsync();


                ChatCompletionResponse completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responsedata, jsSerializerOption);
                var res = completionResponse.Choices[0];

                if (res.FinishReason == "function_call")
                {
                    await HandleFunctionCall(res.Message.function_call, history, tools);
                    var msg = await ExecuteConversationStep(tools, history);
                    history.PushMessage(msg);
                    return msg;
                }
                else
                {
                    return new Message(completionResponse.Choices[0].Message.Content, MessageType.Agent)
                    {
                        InputTokens = completionResponse.Usage.PromptTokens,
                        OutputTokens = completionResponse.Usage.CompletionTokens
                    };
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private OpenAIMessage[] ConvertToOpenAIMessages(Message[] messages)
    {
        return messages?.Select(x => new OpenAIMessage()
        {
            role = x.Type switch
            {
                MessageType.System => MessageRole.System,
                MessageType.User => MessageRole.User,
                MessageType.Agent => MessageRole.Assistant,
                MessageType.FunctionResponse => MessageRole.Function,
                _ => MessageRole._
            },
            Name = x.Type == MessageType.FunctionResponse ? x.FunctionCall.name : null,
            Content = x.Content,
            function_call = (x.FunctionCall != null && x.Type != MessageType.FunctionResponse) ? new Function_Call()
            {
                name = x.FunctionCall?.name,
                arguments = x.FunctionCall?.arguments
            } : null
        }).ToArray();
    }

    public void SetSystemPrompt(string sysPrompt)
    {
        _systemPrompt.Content = sysPrompt;
    }

    private async Task HandleFunctionCall(Function_Call function_call, ConversationHistory history, IEnumerable<ITool> tools)
    {
        history.PushMessage(new Message("", MessageType.Agent)
        {
            FunctionCall = new Message.FuncData() { name = function_call.name, arguments = function_call.arguments },
            IsInternal = true
        });


        Dictionary<string, string> args = JsonSerializer.Deserialize<Dictionary<string, string>>(function_call.arguments);

        var result = await tools.First(x => x.Name == function_call.name).Invoke(args);


        history.PushMessage(new Message(result, MessageType.FunctionResponse)
        {
            FunctionCall = new Message.FuncData() { name = function_call.name, arguments = function_call.arguments },
            IsInternal = true
        });
    }

    public string GetSystemPrompt()
    {
        return _systemPrompt.Content;
    }
}
