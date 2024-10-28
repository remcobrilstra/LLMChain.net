using LLMChain.Core.Conversations;
using LLMChain.Core.Tools;

namespace LLMChain.Core;

public interface IAIProvider
{
    string Key { get; }
    string DisplayName { get; }

    string ActiveModel { get; set; }
    string[] AvailableModels { get; }


    Task<Message> SendChatMessageAsync(Message message,ConversationHistory history, IEnumerable<ITool> tools = null);
    Task<Message> StreamChatMessage(Message message, ConversationHistory history, Action<string> OnStream, IEnumerable<ITool> tools = null);

}
