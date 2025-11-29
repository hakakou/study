using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;


public class S10_OCR : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-5-nano-2025-08-07",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        byte[] bytes = File.ReadAllBytes("media/OCR2.jpg");

        var chatHistory = new ChatHistory("Η δουλειά σου είναι να κάνεις OCR επίσημων εγγράφων. Το έγγραφο πιθανότατα θα είναι στα Ελληνικά. Επίσης γράψε και τον τύπο του εγγράφου.");
        chatHistory.AddUserMessage(
        [
            new ImageContent(bytes, "image/jpeg"),
        ]);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }
}