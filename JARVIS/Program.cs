using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Grpc.Core;
using Grpc.Net.Client;
using JARVIS;
using NAudio.Wave;

var config = Config.Load("config.json");

JARVIS.TextToSpeech textToSpeech = new JARVIS.TextToSpeech(config.GoogleCloudTextToSpeechKey);
SpeechToText speechToText = new SpeechToText(config.GoogleCloudTextToSpeechKey);

OpenAI openAI = new OpenAI(2000);

await BeginBot(textToSpeech, speechToText, openAI);

static async Task BeginBot(JARVIS.TextToSpeech textToSpeech, SpeechToText speechToText, OpenAI openAI)
{
    string opening = "Hello, My name is JARVIS. How can I help you today?";
    Console.WriteLine(opening);
    await textToSpeech.SpeakAsync(opening);

    while (true)
    {
        byte[] audioData = await RecordAudioAsync();

        if (audioData == null)
        {
            Console.WriteLine("Failed to recognize speech input.");
            continue;
        }

        string input = await speechToText.RecognizeSpeechAsync(audioData);

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Failed to recognize speech input.");
            continue;
        }

        if (input.ToLower() == "exit")
        {
            break;
        }

        var output = await GenerateOutput(openAI, input);
        Console.WriteLine($"JARVIS: {output}");

        await textToSpeech.SpeakAsync(output);
    }
}

static async Task<byte[]> RecordAudioAsync()
{
    using (var waveIn = new WaveInEvent())
    using (var memoryStream = new MemoryStream())
    {
        waveIn.WaveFormat = new WaveFormat(16000, 1);

        waveIn.DataAvailable += (sender, e) =>
        {
            memoryStream.Write(e.Buffer, 0, e.BytesRecorded);
        };

        waveIn.StartRecording();

        Console.WriteLine("Listening for input...");
        await Task.Delay(TimeSpan.FromSeconds(5)); // Wait for 5 seconds of audio to be captured

        waveIn.StopRecording();

        if (memoryStream.Length == 0)
        {
            return null;
        }

        return memoryStream.ToArray();
    }
}

static async Task<string> GenerateOutput(OpenAI openAI, string prompt)
{
    var stop = new[] { " Human:", " AI:" };
    var engine = "text-davinci-003";
    var temperature = 0.7;
    var maxTokens = 2000;
    var topP = 1.0;
    var frequencyPenalty = 0.0;
    var presencePenalty = 0.6;

    return await openAI.CreateCompletion(prompt, engine, maxTokens, temperature, topP, frequencyPenalty, presencePenalty, stop);
}