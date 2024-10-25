using LLMChain.Core;
using LLMChain.Core.Conversations;
using LLMChain.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Anthropic
{
    public class AnthropicAIProvider: IAIProvider
    {
        public string Key => "Claude";
        public string DisplayName => "Anthropic AI";

        public bool CanStream => false;
        public bool CanUseTools => false;

        public string[] AvailableModels => throw new NotImplementedException();

        public string ActiveModel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void SetSystemPrompt(string sysPrompt)
        {
            throw new NotImplementedException();
        }

        public string GetSystemPrompt()
        {
            throw new NotImplementedException();
        }

        public Task<Message> SendChatMessageAsync(Message message, ConversationHistory history, IEnumerable<ITool>? tools = null)
        {
            throw new NotImplementedException();
        }

        public Task<Message> StreamChatMessage(Message message, ConversationHistory history, Action<string> OnStream, IEnumerable<ITool>? tools = null)
        {
            throw new NotImplementedException();
        }

    }
}
