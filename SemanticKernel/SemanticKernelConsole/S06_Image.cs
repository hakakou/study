using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

public class S06_Image : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();

        // Load an image from disk.
        byte[] bytes = File.ReadAllBytes("media/animals.webp");
        // byte[] bytesjpg = File.ReadAllBytes("media/BKA5171.jpg");

        var chatHistory = new ChatHistory("Your job is describing images.");
        chatHistory.AddUserMessage(
        [
            new TextContent("What’s in this image?"),
            new ImageContent(bytes, "image/webp"),
        ]);

        //chatHistory.Clear();
        //chatHistory.AddUserMessage(
        //[
        //    new TextContent("Extract structured data: owner name, car type, license plate."),
        //    new ImageContent(bytesjpg, "image/jpg"),
        //]);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
        Console.WriteLine(reply.Content);
    }

    /*
    Prompt tokens: 14190. Completion tokens: 202. Total tokens: 14392.
    The image consists of a grid displaying various marine and aquatic creatures, each in individual boxes. Here's a breakdown of some of the items:

    1. **Top Row**: A starfish, jellyfish, colorful underwater structures, and an octopus.
    2. **Second Row**: Two jellyfish, a spotted species (possibly a type of insect), and a marine worm.
    3. **Third Row**: An image of an armored or microscopic organism (like an isopod), a transparent creature, and an eel.
    4. **Fourth Row**: A crab, a small elongated creature with a red color (possibly a shrimp), and a segmented, colorful marine organism.
    5. **Fifth Row**: A close-up of a spider (likely a jumping spider), a brightly colored slug or sea slug, and an unusual marine creature resembling a sculpture.

    Overall, the image showcases a variety of fascinating and diverse forms of marine life and some terrestrial creatures that are often associated with aquatic environments.
    */
}