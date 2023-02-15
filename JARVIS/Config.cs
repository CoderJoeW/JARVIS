using Newtonsoft.Json.Linq;
using System.IO;

namespace JARVIS
{
    public class Config
    {
        public string OpenAIApiKey { get; set; }

        public static Config Load(string path)
        {
            var json = File.ReadAllText(path);
            var config = JObject.Parse(json);

            return new Config
            {
                OpenAIApiKey = config["OpenAI"]["ApiKey"].ToString()
            };
        }
    }
}
