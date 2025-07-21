using OpenAI.Chat;
using Spectre.Console;

class T03_ZeroShot_Demo2
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        ChatCompletion completion2 = await chatClient.CompleteChatAsync(
        [
            new UserChatMessage(@"Determine at most three topics that are being discussed in the following text, delimited by triple backticks. Each topic should be 1-2 words.
'''Language models have revolutionized the way we interact with technology, empowering us to
generate creative content, explore new ideas, and enhance our communication. LLMs offer immense
potential for unlocking innovation and improving various aspects of our lives, opening up
exciting possibilities for the future.''' ")
        ]);

        // 1.Language Models
        // 2.Innovation
        // 3.Communication
        AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion2.Content[0].Text}");
    }
}
