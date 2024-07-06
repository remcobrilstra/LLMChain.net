using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Core
{
    public interface IAIProvider
    {
        public string DisplayName { get; }
        Task<ChatMessage> SendChatMessageAsync(ChatMessage message, IEnumerable<ITool> tools = null);
        void SetSystemPrompt(string sysPrompt);
        string GetSystemPrompt();

        List<ChatMessage> GetHistory();
        void ClearHistory();

    }
}
