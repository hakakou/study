using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class DOC_S06_Image (IChatCompletionService chatCompletionService) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {

        var chatHistory = new ChatHistory("Your job is describing images.");
        chatHistory.AddUserMessage(
        [
            new TextContent("What’s in this image?"),
            new ImageContent(new Uri("https://placeholder.pagebee.io/api/random/400/300")),
            // or bytes[]
        ]);

        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }

    // The image shows a close-up of a ring-tailed lemur.
}