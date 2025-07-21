using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelConsole.Functions;
using Spectre.Console;

// https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Demos/TimePlugin
public class S02_DemosTimePlugin : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();
        kernel.Plugins.AddFromType<TimePlugin>();

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Enable auto function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            //  Chat history object will be manipulated so that it
            //  includes the function calls and results.
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        AnsiConsole.WriteLine("Ask questions to use the Time Plugin such as: What time is it?");

        ChatHistory chatHistory = [];
        string? input = null;
        while (true)
        {
            input = AnsiConsole.Ask<string>("User > ");
            if (string.IsNullOrWhiteSpace(input))
            {
                // Leaves if the user hit enter without typing any word
                break;
            }

            // Need to add the user message to the chat history
            chatHistory.AddUserMessage(input);
            var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory,
                openAIPromptExecutionSettings, kernel);

            AnsiConsole.MarkupLine($"[red]Assistant:[/] {chatResult}\n");
        }
    }
}