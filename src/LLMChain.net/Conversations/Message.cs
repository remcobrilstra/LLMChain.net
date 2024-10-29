using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Core.Conversations;

public enum MessageType
{
    User,
    Agent,
    System,
    Technical,
    FunctionCall,
    FunctionResponse
}

public class Message
{

    public class FuncData
    {
        public string id;

        public string arguments { get; set; }
        public string name { get; set; }
    }

    public FuncData FunctionCall { get; set; }

    public MessageType Type { get; set; }

    public bool IsInternal { get; set; }

    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public uint InputTokens { get; set; }
    public uint OutputTokens { get; set; }

    public Message(string content, MessageType type = MessageType.User)
    {
        Content = content;
        Type = type;
        Timestamp = DateTime.Now;
    }
}
