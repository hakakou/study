using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System.ComponentModel;

public class S10_OCR : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        byte[] bytes = File.ReadAllBytes("media/OCR2.jpg");

        var chatHistory = new ChatHistory("Your job is to OCR official documents. Document will probably be in Greek. Also write the type of document.");
        chatHistory.AddUserMessage(
        [
            new ImageContent(bytes, "image/jpeg"),
        ]);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }

}