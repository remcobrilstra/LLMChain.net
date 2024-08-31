
using LLMChain.Core;
using LLMChain.OpenAI;
using LLMChain.Tools;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = LoadConfiguration();

string systemPrompt = configuration["General:SystemPrompt"] ?? String.Empty;
bool StreamChatResponse = Boolean.Parse(configuration["General:StreamResponse"]);

//small addition to the system prompt, to make sure we are aware of the current date
systemPrompt += $"\n\nTodays date is:{DateTime.Now.ToLongDateString()}\n";


IAIProvider activeProvider = new OpenAIProvider(configuration["OpenAI:ApiKey"], "gpt-4o");

ChatOrchestrator chatOrchestrator = new ChatOrchestrator(activeProvider);

//Set system prompt
chatOrchestrator.SystemPrompt = systemPrompt;

//Add tools
chatOrchestrator.AddTool(new WeatherTool());
chatOrchestrator.AddTool(new BingSearchTool(configuration["Tools:BingSearch:SubscriptionKey"]));
chatOrchestrator.AddTool(new WebPageTool());


//Simple chatbot

SelectModel(activeProvider);

Console.WriteLine($"-------------------");
Console.WriteLine($"Welcome human, you may start your conversation with the almighty AI, you will be served by {chatOrchestrator.AIProvider.DisplayName} today");
Console.WriteLine($"-------------------");


while (true)
{
    Console.Write("[Human]: ");
    string input = Console.ReadLine();


    if (StreamChatResponse)
    {
        Console.Write($"[AI]: ");
        var resp = await chatOrchestrator.StreamChatMessageAsync(new ChatMessage { Content = input }, chunk =>
        {
            Console.Write(chunk);
            Console.Out.Flush();
        });
        Console.WriteLine($" ");
    }
    else
    {
        var resp = await chatOrchestrator.SendChatMessageAsync(new ChatMessage { Content = input });
        Console.WriteLine($"[AI]:{resp.Content}");
    }

}

static IConfiguration LoadConfiguration()
{
    var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
    IConfiguration configuration = builder.Build();
    return configuration;
}

static void SelectModel(IAIProvider activeProvider)
{
    int i = 1;
    Console.WriteLine("Available models:");
    activeProvider.AvailableModels.ToList().ForEach(model =>
    {
        Console.WriteLine($"{i}. {model}");
        i++;
    });

    while (true)
    {
        Console.Write($"Choose your model (1-{i - 1}):");
        if (Int32.TryParse(Console.ReadLine(), out int modelId))
        {
            string modelName = activeProvider.AvailableModels[modelId - 1];
            activeProvider.ActiveModel = modelName;

            Console.WriteLine($"Model set to '{modelName}'");
            break;
        }
        else
        {
            Console.WriteLine($"Thats not a valid model identifier, please try again");
        }
    }
}