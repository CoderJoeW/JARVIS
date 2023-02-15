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

await BeginBot();

static async Task BeginBot()
{
    OpenAI openAI = new OpenAI(2000);

    var config = Config.Load("config.json");

    // Authenticate with Google Cloud using a service account
    var clientBuilder = new TextToSpeechClientBuilder
    {
        CredentialsPath = config.GoogleCloudTextToSpeechKey
    };
    var client = await clientBuilder.BuildAsync();

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "exit")
        {
            break;
        }

        var output = await GenerateOutput(openAI, input);
        Console.WriteLine($"JARVIS: {output}");

        var response = await client.SynthesizeSpeechAsync(new SynthesizeSpeechRequest
        {
            Input = new SynthesisInput
            {
                Text = output
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
