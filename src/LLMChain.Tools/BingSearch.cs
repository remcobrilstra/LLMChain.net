using LLMChain.Core.Tools;
using System.Text;
using System.Text.Json;

namespace LLMChain.Tools
{
    /// <summary>
    /// A tool that allows you to search Bing for information.
    /// Bing logic mostly taken from the Bing API documentation.
    /// NOTE: there could be more intelligent ways to parse the Bing API response.
    /// </summary>
    public class BingSearchTool : ITool
    {
        public string ID => "CORE.SEARCH.BING";
        public string Name => "Search";

        public string Description => "Useful for when you need to answer questions about current events.";

        public ToolArgs Parameters => new ToolArgs()
        {
            ReturnType = "object",
            Required = ["query"],
            Properties = 
            [
                new("query", "The search query you want to search for")
            ]
        };

        private string SubscriptionKey;
        private string BaseUri;

        public BingSearchTool(string subscriptionKey, string baseUri = "https://api.bing.microsoft.com/v7.0/search")
        {
            SubscriptionKey = subscriptionKey;
            BaseUri = baseUri;
        }

        public async Task<string> Invoke(Dictionary<string, string> args)
        {

            var result = await RequestAndParse(args["query"]);
            var webPages = (System.Text.Json.JsonElement)result["webPages"];

            var realResult = JsonSerializer.Deserialize<BingWebPagesResult>(webPages);
            StringBuilder strbld = new StringBuilder();


            foreach (var item in realResult.value)
            {
                strbld.AppendLine("Snippet: " + item.snippet);
                strbld.AppendLine("URL: " + item.url);
            }


            return strbld.ToString();

        }

        // The user's search string.

        // Bing uses the X-MSEdge-ClientID header to provide users with consistent
        // behavior across Bing API calls. See the reference documentation
        // for usage.

        private static string _clientIdHeader = null;

        private const string QUERY_PARAMETER = "?q=";  // Required
        private const string MKT_PARAMETER = "&mkt=";  // Strongly suggested
        private const string RESPONSE_FILTER_PARAMETER = "&responseFilter=";
        private const string COUNT_PARAMETER = "&count=";
        private const string OFFSET_PARAMETER = "&offset=";
        private const string FRESHNESS_PARAMETER = "&freshness=";
        private const string SAFE_SEARCH_PARAMETER = "&safeSearch=";
        private const string TEXT_DECORATIONS_PARAMETER = "&textDecorations=";
        private const string TEXT_FORMAT_PARAMETER = "&textFormat=";
        private const string ANSWER_COUNT = "&answerCount=";
        private const string PROMOTE = "&promote=";


        async Task<HttpResponseMessage> MakeRequestAsync(string queryString)
        {
            var client = new HttpClient();

            // Request headers. The subscription key is the only required header but you should
            // include User-Agent (especially for mobile), X-MSEdge-ClientID, X-Search-Location
            // and X-MSEdge-ClientIP (especially for local aware queries).

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            return await client.GetAsync(BaseUri + queryString);
        }

        public async Task<Dictionary<string, object>> RequestAndParse(string query)
        {
            // Remember to encode query parameters like q, responseFilters, promote, etc.

            var queryString = QUERY_PARAMETER + Uri.EscapeDataString(query);
            queryString += MKT_PARAMETER + "en-us";
            queryString += COUNT_PARAMETER + "10";
            //queryString += RESPONSE_FILTER_PARAMETER + Uri.EscapeDataString("webpages");
            queryString += TEXT_DECORATIONS_PARAMETER + Boolean.TrueString;

            HttpResponseMessage response = await MakeRequestAsync(queryString);

            _clientIdHeader = response.Headers.GetValues("X-MSEdge-ClientID").FirstOrDefault();

            // This example uses dictionaries instead of objects to access the response data.

            var contentString = await response.Content.ReadAsStringAsync();
            Dictionary<string, object> searchResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(contentString);


            return searchResponse;

        }

    }
}


public class BingWebPagesResult
{
    public string webSearchUrl { get; set; }
    public int totalEstimatedMatches { get; set; }
    public Value[] value { get; set; }
}

public class Value
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public DateTime datePublished { get; set; }
    public string datePublishedDisplayText { get; set; }
    public bool isFamilyFriendly { get; set; }
    public string displayUrl { get; set; }
    public string snippet { get; set; }
    public DateTime dateLastCrawled { get; set; }
    public string cachedPageUrl { get; set; }
    public string language { get; set; }
    public bool isNavigational { get; set; }
    public bool noCache { get; set; }
    public string datePublishedFreshnessText { get; set; }
}

