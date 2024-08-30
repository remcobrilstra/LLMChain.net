using LLMChain.Core;
using LLMChain.OpenAI.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LLMChain.Core.ChatMessage;

namespace LLMChain.OpenAI
{
    public class OpenAIProvider : IAIProvider
    {
        public string DisplayName => $"OpenAI - {Model}";


        private SystemMessage _systemPrompt { get; set; } = new SystemMessage();
        private List<OpenAIMessage> _history { get; set; } = new List<OpenAIMessage>();

        private const string API_ENDPOINT = "https://api.openai.com/v1/";

        private string ApiEndpointRoot {get;set;}

        public float Temperature { get; set; } = 0.7f;
        private string ApiKey { get; set; }

        public string Model { get; private set; }


        JsonSerializerOptions jsSerializerOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters ={
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                },

        };

        public OpenAIProvider(string apiKey, string model, string apiEndpoint = API_ENDPOINT)
        {
            ApiEndpointRoot = apiEndpoint;
            ApiKey = apiKey;
            Model = model;
            ClearHistory();
        }

        public async Task<ChatMessage> SendChatMessageAsync(ChatMessage message, IEnumerable<ITool> tools = null)
        {

            _history.Add(new UserMessage()
            {
                role = MessageRole.User,
                Content = message.Content
            });

            await ExecuteConversationStep(tools);

            return new ChatMessage()
            {
                Content = _history.Last().Content,
                Author = _history.Last().role == MessageRole.User ? MessageAuthor.Human : MessageAuthor.AI
            };
        }

        public async Task<ChatMessage> StreamChatMessage(ChatMessage message, Action<string> OnStream, IEnumerable<ITool> tools = null)
        {

            _history.Add(new UserMessage()
            {
                role = MessageRole.User,
                Content = message.Content
            });

            await StreamConversationStep(tools,OnStream);

            return new ChatMessage()
            {
                Content = _history.Last().Content,
                Author = _history.Last().role == MessageRole.User ? MessageAuthor.Human : MessageAuthor.AI
            };
        }


        private async Task StreamConversationStep(IEnumerable<ITool> tools, Action<string> OnStream)
        {
            ChatCompletionRequest request = new ChatCompletionRequest
            {
                messages = _history.ToArray(),
                functions = ConvertInternalToOpenAIFunctions(tools),
                model = Model,
                stream = true,
                temperature = Temperature
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

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("data: "))
                        {
                            var json = line.Substring(6);
                            if (json == "[DONE]") break;

                            var completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(json, jsSerializerOption).Choices[0].Delta;

                            if (completionResponse.function_call != null)
                            {
                                functionmode = true;
                            }

                            if (functionmode)
                            {
                                func.name += completionResponse.function_call?.name;
                                func.arguments += completionResponse.function_call?.arguments;
                            }
                            else
                            {
                                try
                                {
                                    if (!String.IsNullOrWhiteSpace(completionResponse.Content))
                                    {
                                        OnStream(completionResponse.Content);
                                        strbld.Append(completionResponse.Content);
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
                        await HandleFunctionCall(func, tools);
                        await StreamConversationStep(tools,OnStream);
                    }
                    else
                    {
                        _history.Add(new AssistantMessage()
                        {
                            Content = strbld.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        private async Task ExecuteConversationStep(IEnumerable<ITool> tools)
        {

            ChatCompletionRequest request = new ChatCompletionRequest
            {
                messages = _history.ToArray(),
                functions = ConvertInternalToOpenAIFunctions(tools),
                model = Model,
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
                        await HandleFunctionCall(res.Message.function_call, tools);
                        await ExecuteConversationStep(tools);
                    }
                    else
                    {

                        _history.Add(new AssistantMessage()
                        {
                            Content = completionResponse.Choices[0].Message.Content,
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SetSystemPrompt(string sysPrompt)
        {
            _systemPrompt.Content = sysPrompt;
        }

        public List<ChatMessage> GetHistory()
        {
            return new List<ChatMessage>(_history.Where(x => x is UserMessage ||
                                                             x is AssistantMessage).ToList()
                                                 .ConvertAll(x => new ChatMessage()
                                                 {
                                                     Author = x.role == MessageRole.User ? MessageAuthor.Human : MessageAuthor.AI,
                                                     Content = x.Content
                                                 }));
        }

        public void ClearHistory()
        {
            _history.Clear();
            _history.Add(_systemPrompt);
        }



        private async Task HandleFunctionCall(Function_Call function_call, IEnumerable<ITool> tools)
        {
            _history.Add(new OpenAIMessage
            {
                function_call = function_call,
                role = MessageRole.Assistant
            });


            Dictionary<string, string> args = JsonSerializer.Deserialize<Dictionary<string, string>>(function_call.arguments);

            var result = await tools.First(x => x.Name == function_call.name).Invoke(args);


            _history.Add(new OpenAIMessage
            {
                Name = function_call.name,
                Content = result,
                role = MessageRole.Function
            });


        }

        public string GetSystemPrompt()
        {
            return _systemPrompt.Content;
        }
    }
}
