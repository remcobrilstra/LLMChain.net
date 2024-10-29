using LLMChain.Core.Conversations;
using LLMChain.Core;
using LLMChain.OpenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Client.CLI.SampleLogic;
internal class SimpleChat : ITestLogic
{
    public string Name => "Very Basic Chat";

    public void Configure(IConfiguration configuration)
    {


    }

    public async Task Run(IConfiguration configuration)
    {
        var chatOrchestrator = new ChatOrchestrator();
        chatOrchestrator.AddAIProvider(new OpenAIProvider(configuration["OpenAI:ApiKey"]));

        Conversation conv = new Conversation()
        {
            Agent = new Agent()
            {
                Model = "gpt-4o",
                ModelProvider = "OpenAI",
                Name = "JARVIS",
                SystemPrompt = "You are JARVIS, Just A Rather Very Intelligent System"
            }
        };
        conv.Agent.Initialize();
        chatOrchestrator.ActiveConversation = conv;

        while (true)
        {
            Console.Write("[Human]: ");
            string input = Console.ReadLine();
            var resp = await chatOrchestrator.SendChatMessageAsync(new Message(input));
            Console.WriteLine($"[AI]:{resp.Content}");
        }
    }
}
