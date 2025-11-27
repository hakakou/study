using OpenAI.Chat;
using Spectre.Console;

internal class T04_ZeroShot_Demo2_Refinement
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        ChatCompletion completion2 = await chatClient.CompleteChatAsync(
    [
        new UserChatMessage(@"Determine at most three topics that are being discussed in the following text, delimited by triple backticks.
Format the response as a line of at most 2 words, separated by commas.
```Language models have revolutionized the way we interact with technology, empowering us to
generate creative content, explore new ideas, and enhance our communication. LLMs offer immense
potential for unlocking innovation and improving various aspects of our lives, opening up
exciting possibilities for the future.``` ")
    ]);

        // Language models, Innovation, Communication
        AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion2.Content[0].Text}");
    }
}