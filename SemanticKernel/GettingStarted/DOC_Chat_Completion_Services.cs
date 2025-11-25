using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System;
using static SharedConfig.Conf;


public class DOC_Chat_Completion_Services : ITest
{
    public async Task Run()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var kernelBuilder = services.AddKernel()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey
        );

        var serviceProvider = services.BuildServiceProvider();

        // note services == kernelBuilder.Services

        var kernel = serviceProvider.GetRequiredService<Kernel>();
        var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

        ChatHistory history = [];
        history.AddUserMessage("Explain quantum computing in simple terms.");

        Console.Write("Assistant: ");

        await foreach (var message in chatCompletionService.GetStreamingChatMessageContentsAsync(
            history, kernel: kernel))
        {
            Console.Write(message.Content);
        }

        Console.WriteLine();
    }

    private async Task Example3_StandaloneInstance()
    {
        Console.WriteLine("Example 3: Creating standalone chat completion instance");

        OpenAIChatCompletionService chatCompletionService = new(
            modelId: "gpt-4o",
            apiKey: Conf.OpenAI.ApiKey
        );

        ChatHistory history = [];
        history.AddUserMessage("Tell me a short joke.");

        var response = await chatCompletionService.GetChatMessageContentAsync(history);

        Console.WriteLine($"Assistant: {response.Content}");
    }

}
