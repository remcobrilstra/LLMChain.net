// See https://aka.ms/new-console-template for more information
using LLMChain.BlackForestLabs;
using Microsoft.Extensions.Configuration;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var config = LoadConfiguration();

        FluxProvider provider = new FluxProvider(config["BlackForestLabs:ApiKey"]);

        var request = new FluxProvider.ImageGenRequest()
        {
            prompt = "An image of a woman with fair skin and natural, wavy hair styled in soft curls around her face. She has defined eyebrows and light-colored eyes that draw attention. She's dressed in a slytherin hogwarts school outfit, big wizards robe waving behind her. she holds up a wand with a green glowing tip. the woman is standing on a cliff overlooking the great lake showing hogwarts castle on the other side of the lake.",
            width = 1024,
            height = 1024,
            //steps = 40,
            prompt_upsampling = true,
            //seed = 42,
            safety_tolerance = 6
        };

        var resp = await provider.QueueGenerateImage(request);

        var result = await provider.GetResult(resp.id);
        
        while(result.Status != FluxProvider.ImageGenResult.StatusResponse.Ready)
        {
            result = await provider.GetResult(resp.id);
        }
        
        var imgUrl = result.Result["sample"].ToString();

        using (var client = new WebClient())
        {
            client.DownloadFile(imgUrl, $"{resp.id}.jpg");
        }


        Console.WriteLine("Bye, World!");



        static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}