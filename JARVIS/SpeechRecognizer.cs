using Google.Cloud.Speech.V1;

public class SpeechRecognizer
{
    private readonly SpeechClient _client;
    private readonly string _credentialsPath;

    public SpeechRecognizer(string credentialsPath)
    {
        _credentialsPath = credentialsPath;

        _client = new SpeechClientBuilder
        {
            CredentialsPath = credentialsPath
        }.Build();
    }

    public async Task<string> RecognizeSpeechAsync(byte[] audioData)
    {
        var config = new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = 16000,
            LanguageCode = "en-US",
        };

        var audio = RecognitionAudio.FromBytes(audioData);
        var response = await _client.RecognizeAsync(config, audio);

        if (response.Results.Count == 0)
        {
            return null;
        }

        return response.Results[0].Alternatives[0].Transcript.ToLower();
    }
}
