using Google.Cloud.Speech.V1;
using System.Threading.Tasks;

namespace JARVIS
{
    public class SpeechToText
    {
        private readonly SpeechClient _client;

        public SpeechToText(string credentialsPath)
        {
            _client = new SpeechClientBuilder
            {
                CredentialsPath = credentialsPath
            }.Build();
        }

        public async Task<string> RecognizeSpeechAsync(byte[] audioData)
        {
            var response = await _client.RecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = "en-US"
            }, RecognitionAudio.FromBytes(audioData));

            return response.Results.FirstOrDefault()?.Alternatives.FirstOrDefault()?.Transcript ?? "";
        }
    }
}
