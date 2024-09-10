namespace LLMChain.Core.Conversations;
public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public Agent Agent { get; set; }
    public ConversationHistory History { get; set; }

    public Conversation()
    {
        History = new ConversationHistory();
    }

    public double CalculateCost()
    {

        double cost = 0;

        var modelInfo = ModelInformationRepository.Instance[Agent.Model];

        foreach (var message in History.GetFullHistory())
        {
            if (message.InputTokens > 0)
            {
                cost += message.InputTokens * modelInfo.InputTokenCost1M / 1_000_000;
            }
            if (message.OutputTokens > 0)
            {
                cost += message.OutputTokens * modelInfo.OutputTokenCost1M / 1_000_000;
            }

        }
        return cost;
    }

}
