using LLMChain.Core.Conversations;
using LLMChain.Core.Models;
using LLMChain.Core.Tools;

namespace LLMChain.Core;

/// <summary>
/// Simple Orchestrator for Chat implementations using a single AIProvider
/// </summary>
public class ChatOrchestrator
{
    private Dictionary<string,IAIProvider> aIProviders = new Dictionary<string, IAIProvider>();
    public Conversation ActiveConversation { get; set; }
    public ModelInformation[]? ModelInformation {
        get
        {
            return ModelInformationRepository.Instance.Models;
        }
    }

    public ChatOrchestrator()
    {
    }


    public IAIProvider GetAIProvider(string key)
    {
        return aIProviders[key];
    }

    public void AddAIProvider(IAIProvider provider)
    {
        aIProviders.Add(provider.Key, provider);
    }

    public async Task<Message> SendChatMessageAsync(Message message)
    {
        ActiveConversation.History.PushMessage(message);

        //Get the AIProvider from the active conversation
        var AIProvider = aIProviders[ActiveConversation.Agent.ModelProvider];
        AIProvider.ActiveModel = ActiveConversation.Agent.Model;
        ActiveConversation.History.SetSystemPrompt(ActiveConversation.Agent.SystemPrompt);

        var response = await AIProvider.SendChatMessageAsync(message, ActiveConversation.History, ActiveConversation.Agent.Tools);

        ActiveConversation.History.PushMessage(response);
        return response;
    }

    public async Task<Message> StreamChatMessageAsync(Message message, Action<string> OnStream)
    {
        ActiveConversation.History.PushMessage(message);

        //Get the AIProvider from the active conversation
        var AIProvider = aIProviders[ActiveConversation.Agent.ModelProvider];
        AIProvider.ActiveModel = ActiveConversation.Agent.Model;
        ActiveConversation.History.SetSystemPrompt(ActiveConversation.Agent.SystemPrompt);

        var response = await AIProvider.StreamChatMessage(message, ActiveConversation.History, OnStream, ActiveConversation.Agent.Tools);

        ActiveConversation.History.PushMessage(response);
        return response;
    }

    public void NewConversation(string systemPrompt)
    {
        ActiveConversation = new Conversation()
        {
            History = new ConversationHistory()
        };
        ActiveConversation.History.SetSystemPrompt(systemPrompt);
    }
}
