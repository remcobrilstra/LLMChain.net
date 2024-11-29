using LLMChain.Anthropic;
using LLMChain.Core;
using LLMChain.Core.Conversations;
using LLMChain.Core.Models;
using LLMChain.Core.Tools;
using LLMChain.OpenAI;
using LLMChain.Tools;
using Microsoft.Extensions.Configuration;

namespace LLMChain.Client.CLI.SampleLogic;
internal class ClassicChat : ITestLogic
{
    ChatOrchestrator chatOrchestrator;

    public string Name => "Simple Chat Demo";

    public void Configure(IConfiguration configuration)
    {
        LoadModelData(configuration);

        var modelinfo = configuration.GetSection("ModelInfo");
        ModelInformation[] models = modelinfo.Get<ModelInformation[]>();
        ModelInformationRepository.Instance.AddModels(models);

        IAIProvider OpenAiProvider = new OpenAIProvider(configuration["OpenAI:ApiKey"], "gpt-4o");
        IAIProvider XAIProvider = new OpenAIProvider(configuration["xAI:ApiKey"], "grok-beta", "https://api.x.ai/v1/", "xAI");
        IAIProvider AntProvider = new AnthropicAIProvider(configuration["Anthropic:ApiKey"], "claude-3-5-sonnet-20241022");

        chatOrchestrator = new ChatOrchestrator();

        chatOrchestrator.AddAIProvider(OpenAiProvider);
        chatOrchestrator.AddAIProvider(XAIProvider);
        chatOrchestrator.AddAIProvider(AntProvider);

        ToolRepository.Instance.AddTool(new WeatherTool());
        ToolRepository.Instance.AddTool(new BingSearchTool(configuration["Tools:BingSearch:SubscriptionKey"]));
        ToolRepository.Instance.AddTool(new WebPageTool());
        ToolRepository.Instance.AddTool(new OpenBrowser());
    }

    private static void LoadModelData(IConfiguration configuration)
    {
        var modelinfo = configuration.GetSection("ModelInfo");
        ModelInformation[] models = modelinfo.Get<ModelInformation[]>();
        ModelInformationRepository.Instance.AddModels(models);
    }


    string SelectProvider()
    {
        int i = 1;
        Console.WriteLine("Available providers:");
        chatOrchestrator.AIProviders.ToList().ForEach(model =>
        {
            Console.WriteLine($"{i}. {model.Key}");
            i++;
        });

        while (true)
        {
            Console.Write($"Choose your provider (1-{i - 1}):");
            if (int.TryParse(Console.ReadLine(), out int modelId))
            {
                return chatOrchestrator.AIProviders[modelId - 1].Key;
            }
            else
            {
                Console.WriteLine($"Thats not a valid provider identifier, please try again");
            }
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

    async Task ChatLogicLoop()
    {
        Console.WriteLine($"-------------------");
        Console.WriteLine($"Welcome human, you may start your conversation with the almighty AI, you will be served by '{chatOrchestrator.ActiveConversation.Agent.Name}' today");
        Console.WriteLine($"-------------------");


        while (true)
        {
            Console.WriteLine($"Cost: ${chatOrchestrator.ActiveConversation.CalculateCost()}");
            Console.Write("[Human]: ");
            string input = Console.ReadLine();

            if (chatOrchestrator.GetAIProvider(chatOrchestrator.ActiveConversation.Agent.ModelProvider).CanStream)
            {
                Console.Write($"[AI]: ");
                var resp = await chatOrchestrator.StreamChatMessageAsync(new Message(input), chunk =>
                {
                    Console.Write(chunk);
                    Console.Out.Flush();
                });
                Console.WriteLine($" ");
            }
            else
            {
                var resp = await chatOrchestrator.SendChatMessageAsync(new Message(input));
                Console.WriteLine($"[AI]:{resp.Content}");
            }

        }
    }

    public async Task Run(IConfiguration configuration)
    {

        string provider = SelectProvider();

        string model = SelectModel(provider);


        Conversation conv = new Conversation()
        {
            Agent = new Agent()
            {
                Model = model, //"grok-beta", //"gpt-4o",
                ModelProvider = provider,
                Name = "Tesla Optimus",
                SystemPrompt = "You are a tesla optimus, friendly general assistance robot, you live to serve",
                ToolIds = [ "CORE.WEATHER"]
            }
        };
        conv.Agent.Initialize();
        chatOrchestrator.ActiveConversation = conv;

        await ChatLogicLoop();
    }
}
