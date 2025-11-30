using Azure.AI.OpenAI;
using OpenAI.Chat;
using SharedConfig;
using Spectre.Console;
using System.ClientModel;

internal class Program
{
    public static AzureOpenAIClient AzureClient;
    public static ChatClient ChatClient;

    private static async Task Main(string[] args)
    {
        Conf.Init<Program>();

        var apiKey = Conf.AzureFoundry.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            AnsiConsole.MarkupLine("[red]API key not found. Please set the AzureOpenAI:ApiKey environment variable.[/]");
            return;
        }

        AzureClient = new AzureOpenAIClient(
            new Uri(Conf.AzureFoundry.Endpoint),
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey));

        ChatClient = AzureClient.GetChatClient(Conf.AzureFoundry.DeploymentName);

        var selectedFunction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a function to run")
                .AddChoices(
                    nameof(T01_Intro),
                    nameof(T02_ZeroShot_Demo1),
                    nameof(T03_ZeroShot_Demo2),
                    nameof(T04_ZeroShot_Demo2_Refinement),
                    nameof(T05_Chatbot_Hotel),
                    nameof(T06_HomemadeFunctionCalling),
                    nameof(T07_FunctionCalling),
                    nameof(T08_Embeddings),
                    nameof(T09_Rag),
                    nameof(T10_Rag_Enhanced)
                ));

        switch (selectedFunction)
        {
            case nameof(T01_Intro):
                await T01_Intro.Run();
                break;

            case nameof(T02_ZeroShot_Demo1):
                await T02_ZeroShot_Demo1.Run();
                break;

            case nameof(T03_ZeroShot_Demo2):
                await T03_ZeroShot_Demo2.Run();
                break;

            case nameof(T04_ZeroShot_Demo2_Refinement):
                await T04_ZeroShot_Demo2_Refinement.Run();
                break;

            case nameof(T05_Chatbot_Hotel):
                await T05_Chatbot_Hotel.Run();
                break;

            case nameof(T06_HomemadeFunctionCalling):
                await T06_HomemadeFunctionCalling.Run();
                break;

            case nameof(T07_FunctionCalling):
                await T07_FunctionCalling.Run();
                break;

            case nameof(T08_Embeddings):
                await T08_Embeddings.Run();
                break;

            case nameof(T09_Rag):
                await T09_Rag.Run();
                break;

            case nameof(T10_Rag_Enhanced):
                await T10_Rag_Enhanced.Run();
                break;
        }
    }
}