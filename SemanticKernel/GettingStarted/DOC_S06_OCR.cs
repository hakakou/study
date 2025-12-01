using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public class DOC_S06_OCR(IChatCompletionService chatCompletionService) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {
        byte[] bytes = File.ReadAllBytes("media/OCR2.jpg");

        var chatHistory = new ChatHistory("Η δουλειά σου είναι να κάνεις OCR επίσημων εγγράφων. Το έγγραφο πιθανότατα θα είναι στα Ελληνικά. Επίσης γράψε και τον τύπο του εγγράφου.");
        chatHistory.AddUserMessage(
        [
            new ImageContent(bytes, "image/jpeg"),
        ]);

        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }
}