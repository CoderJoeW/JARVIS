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
        private readonly HttpClient _httpClient;
        private readonly List<string> _history = new List<string>();

        public OpenAI()
        {
            var configJson = File.ReadAllText("config.json");
            var config = JObject.Parse(configJson);

            _apiKey = config["OpenAI"]["ApiKey"].ToString();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> CreateCompletion(string prompt, string engine, int maxTokens, double temperature, double topP, double frequencyPenalty, double presencePenalty, string[] stop)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions");
            request.Content = new StringContent($"{{\"model\": \"{engine}\", \"prompt\": \"{GetPrompt(prompt)}\", \"temperature\": {temperature.ToString(CultureInfo.InvariantCulture)}, \"max_tokens\": {maxTokens}, \"top_p\": {topP.ToString(CultureInfo.InvariantCulture)}, \"frequency_penalty\": {frequencyPenalty.ToString(CultureInfo.InvariantCulture)}, \"presence_penalty\": {presencePenalty.ToString(CultureInfo.InvariantCulture)}, \"stop\": {JsonArray(stop)}, \"n\": 1}}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = ParseResponse(responseContent);
                UpdateHistory(GetPrompt(prompt));
                return result;
            }
            else
            {
                string errorMessage;
                try
                {
                    var errorObject = JObject.Parse(responseContent);
                    errorMessage = errorObject?["error"]?["message"]?.ToString() ?? responseContent;
                }
                catch (Exception)
                {
                    errorMessage = responseContent;
                }
                throw new Exception($"OpenAI request failed with status code {response.StatusCode}: {errorMessage}");
            }
        }

        public void Shutdown()
        {
            _httpClient.Dispose();
        }

        private static JArray JsonArray(string[] array)
        {
            return new JArray(array);
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
