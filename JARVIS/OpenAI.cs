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
    class OpenAI: IDisposable
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
            var promptSegments = SplitPrompt(prompt, maxTokens - 50); // Subtract 50 to allow room for the prompt from the previous segment
            var outputSegments = new List<string>();

            foreach (var promptSegment in promptSegments)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions")
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            model = engine,
                            prompt = _history.GetFullPrompt(promptSegment),
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
                outputSegments.Add(result);
            }

            var output = string.Join("", outputSegments);
            _history.Add("AI", output);
            return output;
        }

        private static IEnumerable<string> SplitPrompt(string prompt, int maxLength)
        {
            if (prompt.Length <= maxLength)
            {
                yield return prompt;
                yield break;
            }

            var words = prompt.Split(' ');
            var sb = new StringBuilder();
            foreach (var word in words)
            {
                if (sb.Length + word.Length + 1 > maxLength)
                {
                    yield return sb.ToString().TrimEnd();
                    sb.Clear();
                }
                sb.Append(word).Append(' ');
            }

            if (sb.Length > 0)
            {
                yield return sb.ToString().TrimEnd();
            }
        }

        public void Dispose()
        {
            _httpHandler.Dispose();
        }
    }
}
