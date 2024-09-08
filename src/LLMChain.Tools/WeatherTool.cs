using LLMChain.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Tools
{

    /// <summary>
    /// First sample of a tool/function.
    /// WARNING, weather forcasts provided by this tool may not be accurate,only use in case of sunny weather.
    /// </summary>
    public class WeatherTool: ITool
    {
        public string ID => "CORE.WEATHER";
        public string Name => "Weather";
        public string Description => "Get the weather for a location";
        public ToolArgs Parameters => new ToolArgs
        {
            ReturnType = "object",
            Properties =
            [
                new ("location", "The location to get the weather for" )
            ]
        };

        public async Task<string> Invoke(Dictionary<string, string> args)
        {

            // Get the weather for the location
            var weather = await GetWeather(args["location"]);

            // Return the weather
            return weather;
        }

        private async Task<string> GetWeather(string location)
        {
            // Get the weather for the location
            return "The weather in " + location + " is sunny";
        }
    }
}
