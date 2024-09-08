
using LLMChain.Core;
using LLMChain.Core.Conversations;
using LLMChain.Core.Models;
using LLMChain.Core.Tools;
using LLMChain.OpenAI;
using LLMChain.Tools;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static IConfiguration configuration;
    static ChatOrchestrator chatOrchestrator;
    private static async Task Main(string[] args)
    {
        LoadConfiguration();

        ConfigureOrchestrator();


        //string model = SelectModel();


        string systemPrompt = configuration["General:SystemPrompt"] ?? string.Empty;
        //small addition to the system prompt, to make sure we are aware of the current date
        systemPrompt += $"\n\nTodays date is:{DateTime.Now.ToLongDateString()}\n";

        Conversation conv = new Conversation()
        {
            Agent = new Agent()
            {
                Model = "gpt-4o-mini", //"gpt-4o",
                ModelProvider = "OpenAI",
                Name = "J.A.R.V.I.S.",
                SystemPrompt = systemPrompt,
                ToolIds = new string[] { "CORE.WEBSCRAPE", "CORE.WEATHER", "CORE.SEARCH.BING", "CORE.ACTION.BROWSER" }
            }
        };
        conv.Agent.Initialize();

        chatOrchestrator.ActiveConversation = conv;


        await ChatLogicLoop();
    }

    static void ConfigureOrchestrator()
    {
        var modelinfo = configuration.GetSection("ModelInfo");
        ModelInformation[] models = modelinfo.Get<ModelInformation[]>();
        ModelInformationRepository.Instance.AddModels(models);

        IAIProvider OpenAiProvider = new OpenAIProvider(configuration["OpenAI:ApiKey"], "gpt-4o");

        chatOrchestrator = new ChatOrchestrator();

        chatOrchestrator.AddAIProvider(OpenAiProvider);
        chatOrchestrator.ModelInformation = models;

        ToolRepository.Instance.AddTool(new WeatherTool());
        ToolRepository.Instance.AddTool(new BingSearchTool(configuration["Tools:BingSearch:SubscriptionKey"]));
        ToolRepository.Instance.AddTool(new WebPageTool());
        ToolRepository.Instance.AddTool(new OpenBrowser());

    }

    static void LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        configuration = builder.Build();
    }

    static string SelectModel()
    {
        int i = 1;
        Console.WriteLine("Available models:");
        var provider = chatOrchestrator.GetAIProvider("OpenAI");
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

    static async Task ChatLogicLoop()
    {
        Console.WriteLine($"-------------------");
        Console.WriteLine($"Welcome human, you may start your conversation with the almighty AI, you will be served by '{chatOrchestrator.ActiveConversation.Agent.Name}' today");
        Console.WriteLine($"-------------------");


        while (true)
        {
            Console.WriteLine($"Cost: ${chatOrchestrator.ActiveConversation.CalculateCost()}");
            Console.Write("[Human]: ");
            string input = Console.ReadLine();


            if (bool.Parse(configuration["General:StreamResponse"]))
            {
                Console.Write($"[AI]: ");
                var resp = await chatOrchestrator.StreamChatMessageAsync(new Message(input) { Type = Message.MessageType.User }, chunk =>
                {
                    Console.Write(chunk);
                    Console.Out.Flush();
                });
                Console.WriteLine($" ");
            }
            else
            {
                var resp = await chatOrchestrator.SendChatMessageAsync(new Message(input) { Type = Message.MessageType.User });
                Console.WriteLine($"[AI]:{resp.Content}");
            }

        }
    }
}