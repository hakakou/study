using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelConsole.Functions;

public class S07_ManualFunctionInvocation : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        // Build the kernel
        Console.WriteLine("Building the kernel...");
        var kernel = builder.Build();

        // Add plugins
        Console.WriteLine("Adding plugins...");
        kernel.Plugins.AddFromType<LightsPlugin>();
        kernel.Plugins.AddFromType<TimePlugin>();

        //# Manual Function Invocation
        Console.WriteLine("Setting up manual function invocation...");

        var manualSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: false)
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var manualHistory = new ChatHistory();
        manualHistory.AddUserMessage("Given the current time of day, which of my lights should be on?");

        Console.WriteLine("Starting manual function invocation loop...");
        while (true)
        {
            Console.WriteLine("Requesting chat message content...");
            var result = await chatCompletionService.GetChatMessageContentAsync(
                manualHistory,
                manualSettings,
                kernel);

            if (result.Content != null)
            {
                Console.WriteLine("Manual Function Invocation Response:");
                Console.WriteLine(result.Content);
                break;
            }

            Console.WriteLine("Adding AI response to chat history...");
            manualHistory.Add(result);

            var functionCalls = FunctionCallContent.GetFunctionCalls(result);
            if (!functionCalls.Any())
            {
                Console.WriteLine("No function calls found. Exiting loop...");
                break;
            }

            foreach (var functionCall in functionCalls)
            {
                try
                {
                    Console.WriteLine($"Invoking function: {functionCall.Id}...");
                    var functionResult = await functionCall.InvokeAsync(kernel);
                    Console.WriteLine("Function invocation successful. Adding result to chat history...");
                    manualHistory.Add(functionResult.ToChatMessage());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Function invocation failed: {ex.Message}. Adding exception to chat history...");
                    manualHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
                }
            }
        }
        Console.WriteLine("Manual function invocation loop ended.");
    }
}