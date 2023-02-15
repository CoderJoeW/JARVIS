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

var clientBuilder = new TextToSpeechClientBuilder
{
    CredentialsPath = config.GoogleCloudTextToSpeechKey
};
TextToSpeechClient client = await clientBuilder.BuildAsync();

await BeginBot(client);

static async Task BeginBot(TextToSpeechClient client)
{
    string opening = "Hello, My name is JARVIS. How can I help you today?";
    Console.WriteLine(opening);
    await SpeakText(client, opening);

    OpenAI openAI = new OpenAI(2000);

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "exit")
        {
            break;
        }

        var output = await GenerateOutput(openAI, input);
        Console.WriteLine($"JARVIS: {output}");

        await SpeakText(client, output);
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

static async Task SpeakText(TextToSpeechClient client, string text)
{
    var response = await client.SynthesizeSpeechAsync(new SynthesizeSpeechRequest
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

    using var audioStream = new MemoryStream(response.AudioContent.ToByteArray());
    using var audioFile = new WaveFileReader(audioStream);
    using var outputDevice = new WaveOutEvent();

    outputDevice.Init(audioFile);
    outputDevice.Play();

    while (outputDevice.PlaybackState == PlaybackState.Playing)
    {
        await Task.Delay(100);
    }
}
