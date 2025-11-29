using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

[RunDirectly]
public class DOC_S01_Chat_Completion_Services(IChatCompletionService chat, Kernel kernel)
    : ITestBuilder
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey
        );
    }

    public async Task Run(IServiceProvider serviceProvider)
    {
        // Generates logs, traces and metrics
        await kernel.InvokePromptAsync("Why is the sky blue in one sentence?");

        ChatHistory history = [];
        history.AddUserMessage("Explain quantum computing in simple terms.");

        // Generates partial traces and metrics but no logs
        //var response = await chat.GetChatMessageContentAsync(history, kernel: kernel);
        //Console.WriteLine(response.Content);

        // Generates only partial trances
        //await foreach (var message in chat.GetStreamingChatMessageContentsAsync(
        //    history, kernel: kernel))
        //{
        //    Console.Write(message.Content);
        //}
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