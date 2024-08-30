namespace LLMChain.Core;

public interface IAIProvider
{
    public string DisplayName { get; }
    Task<ChatMessage> SendChatMessageAsync(ChatMessage message, IEnumerable<ITool> tools = null);
    Task<ChatMessage> StreamChatMessage(ChatMessage message, Action<string> OnStream, IEnumerable<ITool> tools = null);

    void SetSystemPrompt(string sysPrompt);
    string GetSystemPrompt();

    List<ChatMessage> GetHistory();
    void ClearHistory();

}
