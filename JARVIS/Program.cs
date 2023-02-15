using System;
using System.Globalization;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using JARVIS;

await BeginBot();

static async Task BeginBot()
{
    Console.WriteLine("Hello! My name is JARVIS. How can I help you?");
    OpenAI openAI = new OpenAI(2000); // Replace with your API key

    SpeechSynthesizer synth = new SpeechSynthesizer();

    synth.Speak("Hello! My name is JARVIS. How can I help you?");

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "exit")
        {
            break;
        }

        var output = await GenerateOutput(openAI, input);
        Console.WriteLine($"JARVIS: {output}");
        synth.Speak(output);
    }

    openAI.Shutdown();
}

static async Task<string> GenerateOutput(OpenAI openAI, string prompt)
{
    var stop = new[] { " Human:", " AI:" };
    var engine = "text-davinci-002";
    var temperature = 0.7;
    var maxTokens = 2000;
    var topP = 1.0;
    var frequencyPenalty = 0.0;
    var presencePenalty = 0.6;

    return await openAI.CreateCompletion(prompt, engine, maxTokens, temperature, topP, frequencyPenalty, presencePenalty, stop);
}
