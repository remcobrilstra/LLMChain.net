using LLMChain.Core.Conversations;
using LLMChain.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLMChain.Core.Models;
using LLMChain.Core.Tools;
using LLMChain.OpenAI;
using LLMChain.Tools;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;

namespace LLMChain.Client.CLI.SampleLogic;
internal class CreateLMStudioAgent : ITestLogic
{
    private ChatOrchestrator chatOrchestrator;

    public string Name => "LmStudio example (make sure its running)";
    public void Configure(IConfiguration configuration)
    {
        chatOrchestrator = new ChatOrchestrator();
        chatOrchestrator.AddAIProvider(new OpenAIProvider("lm-studio", String.Empty , "http://localhost:1234/v1/", "LMStudio"));

        Conversation conv = new Conversation()
        {
            Agent = new Agent()
            {
                ModelProvider = "LMStudio",
                Name = "Tesla Optimus",
                SystemPrompt = "You are a tesla optimus, friendly general assistance robot, you live to serve"
            }
        };
        
        chatOrchestrator.ActiveConversation = conv;
    }

    public async Task Run(IConfiguration configuration)
    {

        string model = SelectModel("LMStudio");

        chatOrchestrator.ActiveConversation.Agent.Model = model;
        chatOrchestrator.ActiveConversation.Agent.Initialize();

        Console.Clear();


        while (true)
        {
            Console.Write("[Human]: ");
            string input = Console.ReadLine();

            Console.Write($"[AI]: ");
            var resp = await chatOrchestrator.StreamChatMessageAsync(new Message(input), chunk =>
            {
                Console.Write(chunk);
                Console.Out.Flush();
            });
            Console.WriteLine($" ");
        }
    }


    string SelectModel(string providerId)
    {
        int i = 1;
        Console.WriteLine("Available models:");
        var provider = chatOrchestrator.GetAIProvider(providerId);
        provider.AvailableModels.ToList().ForEach(model =>
        {
            Console.WriteLine($"{i}. {model}");
            i++;
        });

        while (true)
        {
            Console.Write($"Choose your model (1-{i - 1}):");
            if (int.TryParse(Console.ReadLine(), out int modelId))
            {
                return provider.AvailableModels[modelId - 1];
            }
            else
            {
                Console.WriteLine($"Thats not a valid model identifier, please try again");
            }
        }
    }

}
