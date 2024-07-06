using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Core
{
    public class ChatMessage
    {
        public enum MessageAuthor
        {
            Human,
            AI
        }

        public MessageAuthor Author { get; set; } = MessageAuthor.Human;
        public string Content { get; set; }
    }
}
