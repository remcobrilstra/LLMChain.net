using LLMChain.Core;
using System.Text.RegularExpressions;

namespace LLMChain.Sample.Tools
{
    /// <summary>
    /// A extremely crappy way to retrieve the full content of a webpage
    /// filters out tags and scripts from the page in an attempt to get the content to fit the token limit
    /// </summary>
    public class WebPageTool : ITool
    {
        public string Name => "WebPage";
        public string Description => "Retrieves the full content of a given webpage so it may be analyzed";
        public ToolArgs Parameters => new ToolArgs
        {
            ReturnType = "object",
            Properties = new PropertyDescription[]
            {
                new PropertyDescription ("url") {Description = "the url of the webpage that you want to access" }
            }
        };

        public async Task<string> Invoke(Dictionary<string, string> args)
        {

            // Get the weather for the location
            var page = await GetPage(args["url"]);

            // Return the weather
            return page;
        }

        private async Task<string> GetPage(string url)
        {

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                return StripHTML(await response.Content.ReadAsStringAsync());
            }
        }

        public static string StripHTML(string input)
        {
            string result = input;
            result = Regex.Replace(result, "<head(.*?)</head>", String.Empty, RegexOptions.Singleline);
            result = Regex.Replace(result, "<script(.*?)</script>", String.Empty, RegexOptions.Singleline);
            result = Regex.Replace(result, "<style(.*?)</style>", String.Empty, RegexOptions.Singleline);
            result = Regex.Replace(result, "<a(.*?)</a>", String.Empty, RegexOptions.Singleline);
            result = Regex.Replace(result, "<.*?>", String.Empty, RegexOptions.Singleline);

            result = Regex.Replace(result, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);

            return result;
        }
    }
}
