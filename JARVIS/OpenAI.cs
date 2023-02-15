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
            _httpHandler = new HttpHandler(config.OpenAIApiKey);
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
                var errorMessage = JObject.Parse(rc)?["error"]?["message"]?.ToString() ?? rc;
                throw new Exception($"OpenAI request failed with status code {response.StatusCode}: {errorMessage}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JObject.Parse(responseContent)?["choices"]?.FirstOrDefault()?["text"]?.ToString() ?? throw new Exception("OpenAI response is missing 'text' property");
            _history.Add(prompt);
            return result;
        }

        public void Shutdown()
        {
            _httpHandler.Dispose();
        }
    }
}
