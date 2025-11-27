using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class DOC_S06_Image : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var chatHistory = new ChatHistory("Your job is describing images.");
        chatHistory.AddUserMessage(
        [
            new TextContent("What’s in this image?"),
            new ImageContent(new Uri("https://placeholder.pagebee.io/api/random/400/300")),
            // or bytes[]
        ]);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }

    // The image shows a close-up of a ring-tailed lemur.
}