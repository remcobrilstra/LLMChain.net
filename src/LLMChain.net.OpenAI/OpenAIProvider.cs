using LLMChain.Core;
using LLMChain.OpenAI.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LLMChain.Core.ChatMessage;

namespace LLMChain.OpenAI
{
    public class OpenAIProvider : IAIProvider
    {
        public string DisplayName => $"OpenAI - {Model}";


        private SystemMessage _systemPrompt { get; set; } = new SystemMessage();
        private List<OpenAIMessage> _history { get; set; } = new List<OpenAIMessage>();

        private const string API_ENDPOINT = "https://api.openai.com/v1/chat/completions";
        private string ApiKey { get; set; }

        public string Model { get; private set; }

        public OpenAIProvider(string apiKey, string model)
        {
            ApiKey = apiKey;
            Model = model;
            ClearHistory();
        }

        public async Task<ChatMessage> SendChatMessageAsync(ChatMessage message, IEnumerable<ITool> tools = null)
        {

            _history.Add(new UserMessage()
            {
                role = MessageRole.User,
                Content = message.Content
            });

            await ExecuteConversationStep(tools);

            return new ChatMessage()
            {
                Content = _history.Last().Content,
                Author = _history.Last().role == MessageRole.User ? MessageAuthor.Human : MessageAuthor.AI
            };
        }

        private async Task ExecuteConversationStep(IEnumerable<ITool> tools)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters ={
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                },

            };

            ChatCompletionRequest request = new ChatCompletionRequest
            {
                messages = _history.ToArray(),
                functions = tools?.Select(x => new Models.Function()
                {
                    name = x.Name,
                    description = x.Description,
                    parameters = new Parameters()
                    {
                        properties = x.Parameters.Properties.Select(y => new KeyValuePair<string, Models.propDescription>(y.Name, new propDescription()
                        {
                            description = y.Description,
                            type = y.Type
                        })).ToDictionary(),
                        type = x.Parameters.ReturnType,
                        required = x.Parameters.Required
                    }
                }).ToArray(),
                model = Model,
                stream = false,
                temperature = 0.7f
            };
            try
            {
                using (HttpClient wClient = new HttpClient())
                {
                    wClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);
                    var response = await wClient.PostAsJsonAsync(API_ENDPOINT, request, options);

                    string requestdata = await response.RequestMessage.Content.ReadAsStringAsync();
                    string responsedata = await response.Content.ReadAsStringAsync();


                    ChatCompletionResponse completionResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responsedata, options);
                    var res = completionResponse.Choices[0];

                    if (res.FinishReason == "function_call")
                    {
                        await HandleFunctionCall(res.Message.function_call, tools);
                        await ExecuteConversationStep(tools);
                    }
                    else
                    {

                        _history.Add(new AssistantMessage()
                        {
                            Content = completionResponse.Choices[0].Message.Content,
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void SetSystemPrompt(string sysPrompt)
        {
            _systemPrompt.Content = sysPrompt;
        }

        public List<ChatMessage> GetHistory()
        {
            return new List<ChatMessage>(_history.Where(x => x is UserMessage ||
                                                             x is AssistantMessage).ToList()
                                                 .ConvertAll(x => new ChatMessage()
                                                 {
                                                     Author = x.role == MessageRole.User ? MessageAuthor.Human : MessageAuthor.AI,
                                                     Content = x.Content
                                                 }));
        }

        public void ClearHistory()
        {
            _history.Clear();
            _history.Add(_systemPrompt);
        }



        private async Task HandleFunctionCall(Function_Call function_call, IEnumerable<ITool> tools)
        {
            _history.Add(new OpenAIMessage
            {
                function_call = function_call,
                role = MessageRole.Assistant
            });


            Dictionary<string, string> args = JsonSerializer.Deserialize<Dictionary<string, string>>(function_call.arguments);

            var result = await tools.First(x => x.Name == function_call.name).Invoke(args);


            _history.Add(new OpenAIMessage
            {
                Name = function_call.name,
                Content = result,
                role = MessageRole.Function
            });


        }

        public string GetSystemPrompt()
        {
            return _systemPrompt.Content;
        }
    }
}
