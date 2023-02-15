using System;
using System.Globalization;
using System.IO;
using System.Speech.Recognition;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.TextToSpeech.V1;
using Grpc.Auth;
using Grpc.Core;
using Grpc.Net.Client;
using JARVIS;
using NAudio.Wave;

var config = Config.Load("config.json");

SpeechHandler speechHandler = new SpeechHandler(
    config.GoogleCloudTextToSpeechKey
);

OpenAI openAI = new OpenAI(2000);

await BeginBot(speechHandler, openAI, config);

static async Task BeginBot(SpeechHandler speechHandler, OpenAI openAI, Config config)
{
    string opening = "Hello, My name is JARVIS. How can I help you today?";
    Console.WriteLine(opening);
    await speechHandler.SpeakAsync(opening);

    var speechRecognizer = new SpeechRecognizer(config.GoogleCloudTextToSpeechKey);
    var audioRecorder = new AudioRecorder();

    while (true)
    {
        byte[] audioData = await audioRecorder.RecordAudioAsync(TimeSpan.FromSeconds(5));

        if (audioData == null)
        {
            Console.WriteLine("Failed to recognize speech input.");
            continue;
        }

        Console.WriteLine("Checking for trigger word...");
        var triggerWord = await RecognizeTriggerWord(audioData, config.GoogleCloudTextToSpeechKey);

        if (triggerWord == null)
        {
            Console.WriteLine("Failed to recognize trigger word.");
            continue;
        }

        Console.WriteLine($"Trigger word recognized: {triggerWord}");

        byte[] remainingAudioData = await audioRecorder.RecordAudioAsync(TimeSpan.FromSeconds(5));

        if (remainingAudioData == null)
        {
            Console.WriteLine("Failed to recognize speech input.");
            continue;
        }

        string input = await speechRecognizer.RecognizeSpeechAsync(remainingAudioData);

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

        await speechHandler.SpeakAsync(output);
    }
}

static async Task<string> RecognizeTriggerWord(byte[] audioData,string credentials)
{
    var client = new SpeechClientBuilder
    {
        CredentialsPath = credentials
    }.Build();

    var config = new RecognitionConfig
    {
        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
        SampleRateHertz = 16000,
        LanguageCode = "en-US",
        SpeechContexts = {
                    new SpeechContext
                    {
                        Phrases = { "jarvis" },
                    },
                   },
    };
    var audio = RecognitionAudio.FromBytes(audioData);
    var response = await client.RecognizeAsync(config, audio);

    if (response.Results.Count == 0)
    {
        return null;
    }

    return response.Results[0].Alternatives[0].Transcript.ToLower();
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