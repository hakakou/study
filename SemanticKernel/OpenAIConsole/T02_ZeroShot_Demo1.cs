using OpenAI.Chat;
using Spectre.Console;

class T02_ZeroShot_Demo1
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        ChatCompletion completion = await chatClient.CompleteChatAsync(
        [
            new UserChatMessage(@"Extract sentiment from the following text delimited by triple backticks.
'''This is very bad!!''' ")
        ]);

        // The sentiment of the text is negative.
        AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion.Content[0].Text}");
    }
}
