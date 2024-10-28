
using LLMChain.Client.CLI;
using LLMChain.Client.CLI.SampleLogic;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static IConfiguration configuration;
    private static async Task Main(string[] args)
    {
        configuration = Util.LoadConfiguration();

        ClassicChat classicChat = new ClassicChat();

        classicChat.Configure(configuration);

        await classicChat.Run(configuration);
    }


}