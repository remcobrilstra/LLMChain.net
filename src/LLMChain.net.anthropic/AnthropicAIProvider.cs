using LLMChain.Core;
using LLMChain.Core.Conversations;
using LLMChain.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static LLMChain.Anthropic.AnthropicAIProvider.AntMessage;
using static LLMChain.Anthropic.AnthropicAIProvider.AntFunctionDef;
using System.Runtime.CompilerServices;
using static LLMChain.Anthropic.AnthropicAIProvider.AntChatCompletionResponse;
using System.Reflection.Metadata;

namespace LLMChain.Anthropic;

public class AnthropicAIProvider : IAIProvider
{
    public string Key => "Claude";
    public string DisplayName => "Anthropic AI";

    public bool CanStream => true;
    public bool CanUseTools => true;

    #region models
    /// <summary>
    /// MODELS
    /// </summary>
    public class AntMessagesCompletionRequest
    {
        public string Model { get; set; }

        public bool Stream { get; set; }
        public int MaxTokens { get; set; }
        public string System { get; set; }
        public AntMessage[] Messages { get; set; }
        public AntFunctionDef[]? Tools { get; internal set; }
    }
    public class AntMessage
    {
        public string role { get; set; }
        public MsgContext[] content { get; set; } //could be an object with image


    }

    public class MsgContext
    { //"id":"toolu_01NKXx61Mps3CZ6r6bhcjuWm","name":"Search","input":{"query":"Claude API tool calling C# implementation"}}
        public string? tool_use_id { get; set; }
        public string? content { get; set; }

        public string type { get; set; }
        public string? text { get; set; }

        public string? id { get; set; }
        public string? name { get; set; }
        public dynamic? input { get; set; }
    }

    public class ErrorResponse
    {
        public string type { get; set; }
        public Error error { get; set; }

        public class Error
        {
            public string type { get; set; }
            public string message { get; set; }
        }
    }


public class ContentBlock{
    public string type { get; set; }
    public string text { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public dynamic input { get; set; }
}

    public class Function_Call
    {
        public string name { get; set; }
        public string arguments { get; set; }
}

public class Error{
    public string type { get; set; }
    public string message { get; set; }
}

public class Message_delta{
    public string type{get;set;}
    public AntChatCompletionResponse delta { get; set; }
    public Usage usage { get; set; }
}

public class Tool_Use{
    public string type {get;set;}
    public string id { get; set; }
    public string name { get; set; }

    public object input { get; set; }

}
public class Tool_use_delta{
    public string type { get; set; }
    public Tool_Use content_block {get;set;}

}

public class partial_json_delta{
    public string type { get; set; }
    public string partial_json {get;set;}
}

public class StreamResponseContentBlock<T>{
    public string type { get; set; }
    public T content_block {get;set;}
}

public class StreamResponseDelta<T>{
    public string type { get; set; }
    public T delta { get; set; }
}


    public class StreamResponse{
        public string type { get; set; }
        public AntChatCompletionResponse message { get; set; }
        public ContentBlock content_block { get; set; }
        public ContentBlock delta { get; set; }

        public Error error { get; set; }
    }
        public class Usage
        {
            public int input_tokens { get; set; }
            public int output_tokens { get; set; }
        }
    public class AntChatCompletionResponse
    {
        public Context[] content { get; set; }
        public string id { get; set; }
        public string model { get; set; }
        public string role { get; set; }
        public string stop_reason { get; set; }
        public object stop_sequence { get; set; }
        public string type { get; set; }
        public Usage usage { get; set; }


        public class Context
        { //"id":"toolu_01NKXx61Mps3CZ6r6bhcjuWm","name":"Search","input":{"query":"Claude API tool calling C# implementation"}}
            public string type { get; set; }
            public string text { get; set; }

            public string id { get; set; }
            public string name { get; set; }
            public dynamic input { get; set; }
        }
    }


    public class AntFunctionDef
    {
        public string name { get; set; }
        public string description { get; set; }
        public Input_Schema input_schema { get; set; }


        public class Input_Schema
        {
            public string type { get; set; } = "object";
            public Dictionary<string, propDescription> properties { get; set; } = new Dictionary<string, propDescription>();
            public string[] required { get; set; }
        }

        public class propDescription
        {
            public propDescription(propType Type = propType.String)
            {
                type = Type;
            }
            public propType type { get; set; }
            public string description { get; set; }
        }
    }


    #endregion

    private string _systemPrompt = "";

    private const string API_ENDPOINT = "https://api.anthropic.com/v1/";

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
    public string[] AvailableModels => ["claude-3-5-sonnet-20241022","claude-3-5-sonnet-20240620", "claude-3-opus-20240229", "claude-3-sonnet-20240229", "claude-3-haiku-20240307"];




    public AnthropicAIProvider(string apiKey, string model = "", string apiEndpoint = API_ENDPOINT)
    {
        ApiEndpointRoot = apiEndpoint;
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
        AntMessagesCompletionRequest request = new AntMessagesCompletionRequest()
        {
            Model = ActiveModel,
            MaxTokens = 8192,
            Stream = true,
            System = history.GetSystemPrompt(),
            Messages = ConvertToAntMessages(history.GetPromptHistory().ToArray()),
            Tools = ConvertInternalToOpenAIFunctions(tools)
        };
        try
        {
            using (HttpClient wClient = new HttpClient())
            {
                wClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                wClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                var response = await wClient.PostAsJsonAsync($"{ApiEndpointRoot}messages", request, jsSerializerOption);

                 using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);



                StringBuilder strbld = new StringBuilder();

                bool functionmode = false;
                Context func = new Context();
                StreamResponse resp = null;
                AntChatCompletionResponse message = null;
                uint inputTokenCost = 0;
                uint outputTokenCost = 0;
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("data: "))
                    {
                        var json = line.Substring(6);

                        resp = JsonSerializer.Deserialize<StreamResponse>(json, jsSerializerOption);

                        switch(resp.type)
                        {
                            case "content_block_start":
                                {
                                    switch(resp.content_block.type)
                                    {
                                        case "tool_use":
                                        {
                                            functionmode = true;
                                            func.input = "";
                                            var tool = JsonSerializer.Deserialize<StreamResponseContentBlock<Tool_Use>>(json, jsSerializerOption);
                                            func.name = tool.content_block.name;
                                            func.id = tool.content_block.id;
                                        }
                                            break;
                                        case "text":
                                            {
                                                OnStream(resp.content_block.text);
                                                strbld.Append(resp.content_block.text);
                                            }
                                            break;
                                    }
                                    continue;
                                }
                                break;
                            case "content_block_delta":
                                {
                                    switch(resp.delta.type)
                                    {
                                        case "input_json_delta":
                                        {
                                            var tool = JsonSerializer.Deserialize<StreamResponseDelta<partial_json_delta>>(json, jsSerializerOption);
                                            func.input += tool.delta.partial_json;
                                        }
                                            break;
                                        case "text_delta":
                                            {
                                                OnStream(resp.delta.text);
                                                strbld.Append(resp.delta.text);
                                            }
                                            break;
                                    }
                                    continue;
                                }
                            case "content_block_stop":
                            {

                                continue;
                            }
                            case "message_start":
                                {
                                    message = resp.message;
                                    if (message.usage != null)
                                    {
                                        outputTokenCost += (uint)message.usage.output_tokens;
                                        inputTokenCost += (uint)message.usage.input_tokens;
                                    }
                                }
                                break;
                            case "message_delta":
                                {
                                    var msg = JsonSerializer.Deserialize<Message_delta>(json, jsSerializerOption);
                                    message = msg.delta;
                                    if (msg.usage != null)
                                    {
                                        outputTokenCost += (uint)msg.usage.output_tokens;
                                        inputTokenCost += (uint)msg.usage.input_tokens;
                                    }
                                }
                                break;
                            case "message_stop":
                              break;
                            case "error":
                                {
                                    if(resp.error.type == "overloaded_error")
                                    {
                                        Thread.Sleep(1000);
                                        return await StreamConversationStep(tools, history, OnStream);
                                    }
                                }
                                break;
                        }
                    }
                    if (line == "event: message_stop") break;
                }

                if (functionmode)
                {
                    await HandleFunctionCall(func, history, tools);
                    var msg = await StreamConversationStep(tools, history, OnStream);
                    history.PushMessage(msg);
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



    private static AntFunctionDef[]? ConvertInternalToOpenAIFunctions(IEnumerable<ITool> tools)
    {
        //throw new NotImplementedException();
        return tools?.Select(x => new AntFunctionDef()
        {
            name = x.Name,
            description = x.Description,
            input_schema = new AntFunctionDef.Input_Schema()
            {
                properties = x.Parameters.Properties.Select(y => new KeyValuePair<string, AntFunctionDef.propDescription>(y.Name, new AntFunctionDef.propDescription()
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

        AntMessagesCompletionRequest request = new AntMessagesCompletionRequest()
        {
            Model = ActiveModel,
            MaxTokens = 8192,
            Stream = false,
            System = history.GetSystemPrompt(),
            Messages = ConvertToAntMessages(history.GetPromptHistory().ToArray()),
            Tools = ConvertInternalToOpenAIFunctions(tools)
        };


        try
        {
            using (HttpClient wClient = new HttpClient())
            {
                wClient.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                wClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                var response = await wClient.PostAsJsonAsync($"{ApiEndpointRoot}messages", request, jsSerializerOption);

                string requestdata = await response.RequestMessage.Content.ReadAsStringAsync();
                string responsedata = await response.Content.ReadAsStringAsync();

                if((int)response.StatusCode == 529)
                {
                    //site overloaded
                    Thread.Sleep(1000);
                    return await ExecuteConversationStep(tools, history);
                }

                AntChatCompletionResponse completionResponse = JsonSerializer.Deserialize<AntChatCompletionResponse>(responsedata, jsSerializerOption);

                if (completionResponse.stop_reason == "tool_use")
                {

                    foreach(var content in completionResponse.content)
                    {
                        switch (content.type)
                        {
                            case "text":
                                {
                                    history.PushMessage(new Message(content.text, MessageType.Agent)
                                    {
                                        IsInternal = false
                                    });
                                }
                                break;
                            case "tool_use":
                                {
                                    await HandleFunctionCall(content, history, tools);
                                }
                                break;
                        }
                    }
                    var msg = await ExecuteConversationStep(tools, history);
                    history.PushMessage(msg);
                    return msg;
                }
                else
                {
                    return new Message(completionResponse.content[0].text, MessageType.Agent)
                    {
                        InputTokens = (uint)completionResponse.usage.input_tokens,
                        OutputTokens = (uint)completionResponse.usage.output_tokens
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

    private AntMessage[] ConvertToAntMessages(Message[] messages)
    {
        //throw new NotImplementedException();
        List<AntMessage> result = new List<AntMessage>();

        foreach(var msg in messages.Where(x => x.Type != MessageType.System))
        {
            var antMsg = new AntMessage();

            antMsg.role = msg.Type switch
            {
                MessageType.User => "user",
                MessageType.Agent => "assistant",
                MessageType.FunctionResponse => "user",
                MessageType.FunctionCall => "assistant",
                _ => ""
            };

            antMsg.content = msg.Type switch
            {
                MessageType.FunctionCall => [new MsgContext() { type = "tool_use", id = msg.FunctionCall.id, name = msg.FunctionCall.name, input = JsonSerializer.Deserialize<dynamic>(msg.FunctionCall.arguments) }],
                MessageType.FunctionResponse => [new MsgContext() { content = msg.Content, tool_use_id = msg.FunctionCall.id, type = "tool_result" }],
                _ => [new MsgContext() { type = "text", text = msg.Content }]
            };

            result.Add(antMsg);
        }

        return result.ToArray();
    }

    private async Task HandleFunctionCall(AntChatCompletionResponse.Context function_call, ConversationHistory history, IEnumerable<ITool> tools)
    {
        history.PushMessage(new Message("", MessageType.FunctionCall)
        {
            FunctionCall = new Message.FuncData() { name = function_call.name, arguments = function_call.input.ToString(), id = function_call.id },
            IsInternal = true
        });


        Dictionary<string, string> args = JsonSerializer.Deserialize<Dictionary<string, string>>(function_call.input.ToString());

        var result = await tools.First(x => x.Name == function_call.name).Invoke(args);


        history.PushMessage(new Message(result, MessageType.FunctionResponse)
        {
            FunctionCall = new Message.FuncData() { name = function_call.name, id = function_call.id, arguments = function_call.input.ToString() },
            IsInternal = true
        });
    }

}