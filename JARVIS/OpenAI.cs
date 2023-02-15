using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JARVIS
{
    class OpenAI
    {
        private readonly string _apiKey;
        private readonly HttpHandler _httpHandler;
        private readonly List<string> _history = new List<string>();

        public OpenAI()
        {
            var configJson = File.ReadAllText("config.json");
            var config = JObject.Parse(configJson);

            _apiKey = config["OpenAI"]["ApiKey"].ToString();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpHandler = new HttpHandler(_apiKey);
        }

        public async Task<string> CreateCompletion(string prompt, string engine, int maxTokens, double temperature, double topP, double frequencyPenalty, double presencePenalty, string[] stop)
        {
            var request = CreateRequest(prompt, engine, maxTokens, temperature, topP, frequencyPenalty, presencePenalty, stop);

            var response = await _httpHandler.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = ParseResponse(responseContent);
                UpdateHistory(GetPrompt(prompt));
                return result;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorMessage = GetErrorMessage(responseContent);
                throw new Exception($"OpenAI request failed with status code {response.StatusCode}: {errorMessage}");
            }
        }

        public void Shutdown()
        {
            _httpHandler.Dispose();
        }

        private HttpRequestMessage CreateRequest(string prompt, string engine, int maxTokens, double temperature, double topP, double frequencyPenalty, double presencePenalty, string[] stop)
        {
            return new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        model = engine,
                        prompt = GetPrompt(prompt),
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

        private void UpdateHistory(string prompt)
        {
            _history.Add(prompt);
            if (_history.Count > 1)
            {
                _history.RemoveRange(0, _history.Count - 1);
            }
        }

        private string GetPrompt(string prompt)
        {
            if (_history.Count == 0)
            {
                return prompt;
            }
            else
            {
                return string.Join("", _history) + prompt;
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
