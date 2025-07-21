using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelConsole.Functions;
using System.ComponentModel;

public class S08_FunctionChoices : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Add plugins (e.g., LightsPlugin and DateTimeUtils)
        kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        kernel.Plugins.AddFromType<TimePlugin>();

        //# Demonstrate Auto Function Choice Behavior
        
        var autoSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var autoHistory = new ChatHistory();
        autoHistory.AddUserMessage("Based on the current time, are the lights correctly on or off?");

        var autoResult = await chatCompletionService.GetChatMessageContentAsync(
            autoHistory,
            executionSettings: autoSettings,
            kernel: kernel);

        Console.WriteLine("Auto Function Choice Behavior Response:");
        Console.WriteLine(autoResult.Content);

        //# Demonstrate Required Function Choice Behavior
        
        var requiredSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
        };

        var requiredHistory = new ChatHistory();
        requiredHistory.AddUserMessage("What are the type of lights that exist?");

        var requiredResult = await chatCompletionService.GetChatMessageContentAsync(
            requiredHistory,
            executionSettings: requiredSettings,
            kernel: kernel);

        Console.WriteLine("Required Function Choice Behavior Response:");
        Console.WriteLine(requiredResult.Content);
        
        //# Demonstrate None Function Choice Behavior
        
        var noneSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.None()
        };

        var noneHistory = new ChatHistory();
        noneHistory.AddUserMessage("Which functions would you use to adjust the lights based on time of day?");

        var noneResult = await chatCompletionService.GetChatMessageContentAsync(
            noneHistory,
            executionSettings: noneSettings,
            kernel: kernel);

        Console.WriteLine("None Function Choice Behavior Response:");
        Console.WriteLine(noneResult.Content);

        /*
        To adjust the lights based on the time of day, I would use the following functions:

        1. **TimeInformationPlugin-GetCurrentUtcTime**: This function retrieves the current time in UTC. Knowing the time allows you to determine what kind of lighting is appropriate (e.g., bright lights during the day, dim lights in the evening).

        2. **functions.Lights-get_lights**: This function gets a list of all lights and their current states. It helps you understand which lights are currently available and their statuses.

        3. **functions.Lights-change_state**: This function changes the state of a specific light (turning it on or off). After determining the time of day and the desired lighting, this function would be used to adjust the lights accordingly.

        Using these functions in sequence would allow for a dynamic adjustment of lighting based on the time of day.
        */
    }
}
