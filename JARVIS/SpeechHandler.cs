using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JARVIS
{
    public class SpeechHandler
    {
        private readonly SpeechClient _speechClient;
        private readonly TextToSpeechClient _ttsClient;

        public SpeechHandler(string speechCredentialsPath)
        {
            _speechClient = new SpeechClientBuilder
            {
                CredentialsPath = speechCredentialsPath
            }.Build();

            _ttsClient = new TextToSpeechClientBuilder
            {
                CredentialsPath = speechCredentialsPath
            }.Build();
        }

        public async Task<string> RecognizeSpeechAsync(byte[] audioData)
        {
            var response = await _speechClient.RecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = "en-US"
            }, RecognitionAudio.FromBytes(audioData));

            return response.Results.FirstOrDefault()?.Alternatives.FirstOrDefault()?.Transcript ?? "";
        }

        public async Task SpeakAsync(string text)
        {
            if (_ttsClient == null)
            {
                throw new ArgumentNullException(nameof(_ttsClient));
            }

            try
            {
                var response = await _ttsClient.SynthesizeSpeechAsync(new SynthesizeSpeechRequest
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
