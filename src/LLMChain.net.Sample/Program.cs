
using LLMChain.Core;
using LLMChain.OpenAI;
using LLMChain.Sample.Tools;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = LoadConfiguration();

string systemPrompt = configuration["General:SystemPrompt"] ?? String.Empty;

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
Console.WriteLine($"-------------------");
Console.WriteLine($"Welcome human, you may start your conversation with the almighty AI, you will be served by {chatOrchestrator.AIProvider.DisplayName} today");
Console.WriteLine($"-------------------");


while (true)
{
    Console.Write("[Human]:");
    string input = Console.ReadLine();

    if (input.ToLower() == "exit")
    {
        break;
    }

    var resp = await chatOrchestrator.SendChatMessageAsync(new ChatMessage { Content = input });

    Console.WriteLine($"[AI]:{resp.Content}");
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