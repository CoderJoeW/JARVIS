using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JARVIS
{
    class OpenAI
    {
        private readonly HttpHandler _httpHandler;
        private readonly History _history;

        public OpenAI(int maxHistoryLength)
        {
            var config = Config.Load("config.json");
            var apiKey = config.OpenAIApiKey;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpHandler = new HttpHandler(apiKey);
            _history = new History(maxHistoryLength);
        }

        public async Task<string> CreateCompletion(string prompt, string engine, int maxTokens, double temperature, double topP, double frequencyPenalty, double presencePenalty, string[] stop)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        model = engine,
                        prompt = _history.GetFullPrompt(prompt),
                        temperature,
                        max_tokens = maxTokens,
                        top_p = topP,
                        frequency_penalty = frequencyPenalty,
                        presence_penalty = presencePenalty,
                        stop = stop,
                        n = 1
                    }),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var response = await _httpHandler.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var rc = await response.Content.ReadAsStringAsync();
                var errorMessage = GetErrorMessage(rc);
                throw new Exception($"OpenAI request failed with status code {response.StatusCode}: {errorMessage}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = ParseResponse(responseContent);
            _history.Add(prompt);
            return result;
        }

        public void Shutdown()
        {
            _httpHandler.Dispose();
        }

        private string GetErrorMessage(string responseContent)
        {
            try
            {
                var errorObject = JObject.Parse(responseContent);
                return errorObject?["error"]?["message"]?.ToString() ?? responseContent;
            }
            catch (Exception)
            {
                return responseContent;
            }
        }

        private static string ParseResponse(string json)
        {
            var jObject = JObject.Parse(json);
            var choices = jObject["choices"].ToObject<List<CompletionChoice>>();
            return choices[0]?.text ?? throw new Exception("OpenAI response is missing 'text' property");
        }
    }

    class CompletionChoice
    {
        public string finish_reason { get; set; }
        public string index { get; set; }
        public string text { get; set; }
    }
}
