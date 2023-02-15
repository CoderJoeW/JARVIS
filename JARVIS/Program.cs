using System;
using System.Globalization;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using JARVIS;

await BeginBot();

static async Task BeginBot()
{
    Console.WriteLine("Hello! My name is JARVIS. How can I help you?");
    OpenAI openAI = new OpenAI(); // Replace with your API key

    SpeechSynthesizer synth = new SpeechSynthesizer();

    synth.Speak("Hello! My name is JARVIS. How can I help you?");

    while (true)
    {
        string input = Console.ReadLine();
        if (input == "exit")
        {
            break;
        }

        string[] stop = { " Human:", " AI:" };
        string prompt = input;
        string engine = "text-davinci-002"; // Updated engine
        double temperature = 0.7; // Updated temperature
        int maxTokens = 2000; // Updated max tokens
        double topP = 1.0;
        double frequencyPenalty = 0.0;
        double presencePenalty = 0.6;

        string output = await openAI.CreateCompletion(prompt, engine, maxTokens, temperature, topP, frequencyPenalty, presencePenalty, stop);

        Console.WriteLine($"JARVIS: {output}");
        synth.Speak(output);
    }

    openAI.Shutdown();
}
