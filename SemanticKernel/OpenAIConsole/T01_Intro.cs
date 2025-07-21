using OpenAI.Chat;
using Spectre.Console;

class T01_Intro
{
    public static async Task Run()
    {
        var chatClient = Program.ChatClient;

        // Display a styled prompt to the user
        string? userQuestion = AnsiConsole.Ask<string>("[yellow]What's your question for the assistant?[/]");

        ChatCompletion completion = await chatClient.CompleteChatAsync(
        [
            new SystemChatMessage("Generate a response with a maximum of 20 words based " +
              "on the given input or context provided. Avoid exceeding word limit."),
            new UserChatMessage(userQuestion),
        ]);

        AnsiConsole.MarkupLine($"[green]Assistant:[/] {completion.Content[0].Text}");
    }
}
