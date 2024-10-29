
using LLMChain.Client.CLI;
using LLMChain.Client.CLI.SampleLogic;
using LLMChain.Core;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

internal class Program
{
    static IConfiguration configuration;
    private static async Task Main(string[] args)
    {

        ITestLogic[] demos = new ITestLogic[]
        {
            new ClassicChat(),
            new CreateXAIAgent()
        };

        var demo = SelectDemo(demos);
        Console.Clear();

        configuration = Util.LoadConfiguration();

        demo.Configure(configuration);

        await demo.Run(configuration);
    }


    private static ITestLogic SelectDemo(ITestLogic[] demos)
    {
        int i = 1;
        Console.WriteLine("Available demos:");
        demos.ToList().ForEach(demo =>
        {
            Console.WriteLine($"{i}. {demo.Name}");
            i++;
        });

        while (true)
        {
            Console.Write($"Choose your demo (1-{i - 1}):");
            if (int.TryParse(Console.ReadLine(), out int modelId))
            {
                return demos[modelId - 1];
            }
            else
            {
                Console.WriteLine($"Thats not a valid demo identifier, please try again");
            }
        }
    }


}