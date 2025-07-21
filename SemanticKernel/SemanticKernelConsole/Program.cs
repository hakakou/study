using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenAI.Chat;
using SharedConfig;
using Spectre.Console;
using System.Net;
using System.Reflection;

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class Program
{
    public static AzureOpenAIClient AzureClient;
    public static ChatClient ChatClient;

    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        Conf.Init<Program>();

        builder.Services.AddOpenAIChatCompletion("gpt-4o-mini",
                apiKey: Conf.OpenAI.ApiKey);

        builder.AddOpenAITextEmbeddingGeneration(
             modelId: "text-embedding-ada-002",
             apiKey: Conf.OpenAI.ApiKey
        );

        //builder.AddOllamaTextEmbeddingGeneration(
        //    modelId: "text-embedding-nomic-embed-text-v1.5",
        //    httpClient: new HttpClient() { BaseAddress = new Uri("http://localhost:1234/v1") }
        //);

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        var testTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ITest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderByDescending(q => q.Name)
            .ToList();

        var directRunType = testTypes.FirstOrDefault(t => t.GetCustomAttribute<RunDirectlyAttribute>() != null);
        if (directRunType != null)
        {
            var instance = (ITest)Activator.CreateInstance(directRunType);
            await instance.Run(builder);
        }
        else
        {
            var selectedFunction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a function to run")
                    .AddChoices(testTypes.Select(t => t.Name).ToArray()));

            var selectedType = testTypes.FirstOrDefault(t => t.Name == selectedFunction);
            if (selectedType != null)
            {
                var instance = (ITest)Activator.CreateInstance(selectedType);
                await instance.Run(builder);
            }
        }

        Console.WriteLine();
        Console.WriteLine();
    }
}

public interface ITest
{
    Task Run(IKernelBuilder builder);
}

public class RunDirectlyAttribute : Attribute
{
}
