using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.OpenAI.Models
{
    class ChatCompletionResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int Created { get; set; }
        public string Model { get; set; }
        public Choice[] Choices { get; set; }
        public Usage Usage { get; set; }
        public string SystemFingerprint { get; set; }

    }

    class Choice
    {
        public uint Index { get; set; }
        public OpenAIMessage Message { get; set; }
        public object Logprobs { get; set; }
        public string FinishReason { get; set; }
    }

    class Usage
    {
        public uint PromptTokens { get; set; }
        public uint CompletionTokens { get; set; }
        public uint TotalTokens { get; set; }
        
    }
}
