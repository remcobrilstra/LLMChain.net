using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
//using Newtonsoft.Json;

namespace LLMChain.BlackForestLabs
{
    public class FluxProvider
    {
        private string ApiKey { get; set; }

        private const string API_ENDPOINT = "https://api.bfl.ml/v1/";

        private string ApiEndpointRoot { get; set; }

        JsonSerializerOptions jsSerializerOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters ={
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },

        };

        public class ImageGenRequest
        {
            public string prompt { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
            public int? steps { get; set; }
            public bool prompt_upsampling { get; set; }
            public int? seed { get; set; } = null;
            public int safety_tolerance { get; set; }
        }

        public class ImageGenResponse
        {
            public string id { get; set; }
        }

            public FluxProvider(string apiKey, string apiRoot = API_ENDPOINT)
        {
            ApiKey = apiKey;
            ApiEndpointRoot = apiRoot;
        }

        public class ImageGenResult
        {
            /// <summary>
            /// Task id for retrieving result
            /// </summary>
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("result")]
            public Dictionary<string, object> Result { get; set; }

            [JsonPropertyName("status")]
            public StatusResponse Status { get; set; }

            public enum StatusResponse { ContentModerated, Error, Pending, Ready, RequestModerated, TaskNotFound };
        }


        public async Task<ImageGenResult> GetResult(string id)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{ApiEndpointRoot}get_result?id={id}"),
                Headers =
                {
                    { "X-Key", ApiKey },
                },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ImageGenResult>(body, jsSerializerOption);
            }

            return null;
        }

        public async Task<ImageGenResponse> QueueGenerateImage(ImageGenRequest requestData, string model = "flux-pro-1.1")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{ApiEndpointRoot}{model}"),
                Headers =
                {
                    { "X-Key", ApiKey },
                },
                Content = new StringContent(JsonSerializer.Serialize(requestData, jsSerializerOption), Encoding.UTF8, "application/json"),
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ImageGenResponse>(body, jsSerializerOption);

            }

            return null;
        }

    }
}
