﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Core.Conversations;
public class Message
{

    public class FuncData
    {
        public string arguments { get; set; }
        public string name { get; set; }
    }

    public FuncData FunctionCall { get; set; }


    public enum MessageType
    {
        User,
        Agent,
        System,
        Technical,
        FunctionResponse
    }

    public MessageType Type { get; set; }

    public bool IsInternal { get; set; }

    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public uint InputTokens { get; set; }
    public uint OutputTokens { get; set; }

    public Message(string content)
    {
        Content = content;
        Timestamp = DateTime.Now;
    }
}
