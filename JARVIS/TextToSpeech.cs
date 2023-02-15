using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace JARVIS
{
    public class TextToSpeech
    {
        private readonly TextToSpeechClient _client;

        public TextToSpeech(string credentialsPath)
        {
            _client = new TextToSpeechClientBuilder
            {
                CredentialsPath = credentialsPath
            }.Build();
        }

        public async Task SpeakAsync(string text)
        {
            if (_client == null)
            {
                throw new ArgumentNullException(nameof(_client));
            }

            try
            {
                var response = await _client.SynthesizeSpeechAsync(new SynthesizeSpeechRequest
                {
                    Input = new SynthesisInput
                    {
                        Text = text
                    },
                    Voice = new VoiceSelectionParams
                    {
                        LanguageCode = "en-US",
                        SsmlGender = SsmlVoiceGender.Male
                    },
                    AudioConfig = new AudioConfig
                    {
                        AudioEncoding = AudioEncoding.Linear16
                    }
                });

                using (var audioStream = new MemoryStream(response.AudioContent.ToByteArray()))
                using (var audioFile = new WaveFileReader(audioStream))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while playing audio: {ex.Message}");
            }
        }
    }
}
