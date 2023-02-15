
# JARVIS

JARVIS (Just A Rather Very Intelligent System) is a simple AI assistant that uses OpenAI's GPT-3 language model to answer user questions and carry out tasks.

## Installation

To run JARVIS, you will need to have .NET installed on your machine. Once you have .NET installed, you can download or clone this repository and open it in Visual Studio.

You will also need to obtain an OpenAI API key in order to use JARVIS. Once you have an API key, you can add it to the `config.json` file.

## Getting started

To use JARVIS, you will need an OpenAI API key. Once you have your API key, add it to the `config.json` file in the following format:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

## Usage

To start JARVIS, simply run the `BeginBot` method in the `Program.cs` file. This will initialize the OpenAI class and begin the chat loop.

During the chat loop, JARVIS will prompt the user for input, then use OpenAI to generate a response. The response will be displayed to the user and read aloud using the SpeechSynthesizer class.

To exit the chat loop, simply type "exit" and hit enter.

Once JARVIS is running, you can type any question or command into the console and JARVIS will respond with the best answer or carry out the requested task. For example:

```
Hello! My name is JARVIS. How can I help you?
> What's the weather like today?
JARVIS: The weather is sunny and mild today.
> Calculate the third degree of a triangle if the first 2 degrees are 20 and 43
JARVIS: 83.6 degrees
> Exit
```

## Classes

The JARVIS chatbot is made up of several C# classes, each of which handles a specific part of the chatbot's functionality.

### OpenAI

The `OpenAI` class is responsible for interfacing with the OpenAI API and generating responses to user input. It takes in several parameters, including the user's input, the OpenAI engine to use, and various settings for the OpenAI API.

### HttpHandler

The `HttpHandler` class is responsible for handling HTTP requests and responses. It takes in an OpenAI API key and uses it to authenticate requests to the OpenAI API.

### History

The `History` class is responsible for maintaining a history of previous user inputs. It takes in a maximum history length and stores a list of previous prompts. When a new prompt is added, the history is truncated to the maximum length.

### Config

The `Config` class is responsible for loading configuration settings from a JSON file. It takes in a file path and returns an object containing the configuration settings.

## Credits

JARVIS was created with a lot of help from ChatGPT

I guess you could say JARVIS helped build JARVIS

## License

This code is released under the MIT License. See LICENSE file for more details.