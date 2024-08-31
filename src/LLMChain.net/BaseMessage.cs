namespace LLMChain.Core;

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
