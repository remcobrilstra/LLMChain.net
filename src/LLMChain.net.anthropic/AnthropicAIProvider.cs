using LLMChain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Anthropic
{
    public class AnthropicAIProvider: IAIProvider
    {
        public string DisplayName => "Anthropic AI";

        public Task<ChatMessage> SendChatMessageAsync(ChatMessage message, IEnumerable<ITool> tools = null)
        {
            throw new NotImplementedException();
        }

        public void SetSystemPrompt(string sysPrompt)
        {
            throw new NotImplementedException();
        }

        public string GetSystemPrompt()
        {
            throw new NotImplementedException();
        }

        public List<ChatMessage> GetHistory()
        {
            throw new NotImplementedException();
        }

        public void ClearHistory()
        {
            throw new NotImplementedException();
        }

        public Task<ChatMessage> StreamChatMessage(ChatMessage message, Action<string> OnStream, IEnumerable<ITool> tools = null)
        {
            throw new NotImplementedException();
        }
    }
}
