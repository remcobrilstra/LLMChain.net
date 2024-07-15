namespace LLMChain.Core;

public class ChatOrchestrator
{
    public IAIProvider AIProvider { get; private set; }

    public List<ITool> Tools { get; private set; } = new List<ITool>();
    private List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();

    public String SystemPrompt
    {
        get
        {
            return AIProvider.GetSystemPrompt();
        }
        set
        {
            AIProvider.SetSystemPrompt(value);
        }
    }


    public ChatOrchestrator(IAIProvider aiProvider)
    {
        AIProvider = aiProvider;
    }

    public void AddTool(ITool tool)
    {
        Tools.Add(tool);
    }
    public void ClearHistory()
    {
        ChatHistory.Clear();
    }

    public async Task<ChatMessage> SendChatMessageAsync(ChatMessage message)
    {
        ChatHistory.Add(message);
        var response = await AIProvider.SendChatMessageAsync(message, Tools);

        ChatHistory.Add(response);
        return response;
    }

    public List<ChatMessage> GetHistory()
    {
        return new List<ChatMessage>(ChatHistory);
    }
}
