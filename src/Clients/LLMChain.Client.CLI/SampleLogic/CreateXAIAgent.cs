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
internal class CreateXAIAgent : ITestLogic
{


    public void Configure(IConfiguration configuration)
    {
        LoadModelData(configuration);

        var chatOrchestrator = new ChatOrchestrator();
        chatOrchestrator.AddAIProvider(new OpenAIProvider(configuration["xAI:ApiKey"], "grok-beta", "https://api.x.ai/v1/", "xAI"));

        Conversation conv = new Conversation()
        {
            Agent = new Agent()
            {
                Model = "grok-beta", //"gpt-4o",
                ModelProvider = "xAI",
                Name = "Tesla Optimus",
                SystemPrompt = "You are a tesla optimus, friendly general assistance robot, you live to serve"
            }
        };
        conv.Agent.Initialize();
        chatOrchestrator.ActiveConversation = conv;
    }

    public Task Run(IConfiguration configuration)
    {
        return Task.FromResult(Task.CompletedTask);
    }

    private static void LoadModelData(IConfiguration configuration)
    {
        var modelinfo = configuration.GetSection("ModelInfo");
        ModelInformation[] models = modelinfo.Get<ModelInformation[]>();
        ModelInformationRepository.Instance.AddModels(models);
    }
}
