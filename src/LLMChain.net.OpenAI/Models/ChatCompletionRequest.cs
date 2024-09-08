using LLMChain.Core.Tools;

namespace LLMChain.OpenAI.Models
{
    class ChatCompletionRequest
    {
        public OpenAIMessage[] messages { get; set; }
        public string model { get; set; }
        public Function[] functions { get; set; }
        public bool stream { get; set; }

        public StreamOptions stream_options { get; set; }
        public float temperature { get; set; }
    }

    public class StreamOptions
    {
        public bool include_usage { get; set; }
    }

    enum MessageRole
    {
        _,
        System,
        User,
        Assistant,
        Tool,
        Function
    }

    class OpenAIMessage 
    {
        public MessageRole role { get; set; }

        public string Content { get; set; }


        public string Name { get; set; }
        public Function_Call function_call { get; set; }
        public uint TokenCost { get; internal set; }
    }

    class SystemMessage : OpenAIMessage
    {
        public SystemMessage()
        {
            role = MessageRole.System;
        }
    }

    class UserMessage : OpenAIMessage
    {
        public UserMessage()
        {
            role = MessageRole.User;
        }
    }

    class AssistantMessage : OpenAIMessage
    {
        public AssistantMessage()
        {
            role = MessageRole.Assistant;
        }

    }
    class Function_Call
    {
        public string arguments { get; set; }
        public string name { get; set; }
    }
    class Function
    {
        public string name { get; set; }
        public string description { get; set; }
        public Parameters parameters { get; set; }
    }

    class Parameters
    {
        public Dictionary<string, propDescription> properties { get; set; } = new Dictionary<string, propDescription>();
        public string[] required { get; set; }
        public string type { get; set; }
    }

    class propDescription
    {
        public propDescription(propType Type = propType.String)
        {
            type = Type;
        }
        public propType type { get; set; }
        public string description { get; set; }
    }
}
