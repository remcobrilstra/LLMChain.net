using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LLMChain.Core.Conversations;

/// <summary>
/// Represents a conversation history
/// </summary>
/// <remarks>
/// TODO:
///  - need to add history strategy to make sure that history will fit context window
///  - system prompt should be handled differently
/// </remarks>
public class ConversationHistory
{

    private Message SystemPrompt = new Message("") { Type = Message.MessageType.System , IsInternal = true};


    public int count { get; set; } = 20;


    private List<Message> Messages { get; set; }

    public ConversationHistory()
    {
        Messages = new List<Message>();
        PushMessage(SystemPrompt);
    }


    public void PushMessage(Message message)
    {
        Messages.Add(message);
    }

    public IReadOnlyCollection<Message> GetFullHistory()
    {
        return Messages.AsReadOnly();
    }

    public IReadOnlyCollection<Message> GetPromptHistory()
    {
        return Messages.TakeLast(count).ToList().AsReadOnly();
    }



    public void ClearHistory()
    {
        //clear all messages, retain system promopt
        Messages.RemoveAll(x=>x.Type != Message.MessageType.System);
    }


    public async Task SaveHistory(string file)
    {
        //save history to file
        using (var fileStream = File.OpenWrite(file))
        {
            await JsonSerializer.SerializeAsync(fileStream, Messages);
        }  
    }

    public static async Task<ConversationHistory> LoadHistory(string file)
    {
        //save history to file
        using (var fileStream = File.OpenRead(file))
        {
            var res = new ConversationHistory();

            res.Messages = await JsonSerializer.DeserializeAsync<List<Message>>(fileStream);

            return res;
        }
    }

    public void SetSystemPrompt(string value)
    {
        SystemPrompt.Content = value;
    }

    public string GetSystemPrompt()
    {
        return SystemPrompt.Content;
    }
}
