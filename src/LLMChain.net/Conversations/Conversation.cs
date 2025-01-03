﻿namespace LLMChain.Core.Conversations;
public class Conversation
{
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
        if(modelInfo == null)
        {
            return 0.0;
        }

        if(modelInfo == null)
        {
            return 0;
        }
        
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
