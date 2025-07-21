using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelConsole.Functions;
using System.ComponentModel;

public class S102_Add_Plugins : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        builder.Plugins.AddFromType<TimePlugin>();
        builder.Plugins.AddFromType<WidgetFactory>();
        Kernel kernel = builder.Build();

        // Example 1. Invoke the kernel with a prompt that asks the AI for information it cannot provide and may hallucinate
        Console.WriteLine(await kernel.InvokePromptAsync(
            "How many days until Christmas?"));

        // Example 2. Invoke the kernel with a templated prompt that invokes a plugin and display the result
        Console.WriteLine(await kernel.InvokePromptAsync(
            "The current time is {{TimePlugin.GetCurrentUtcTime}}. How many days until Christmas?"));

        // Example 3. Invoke the kernel with a prompt and allow the AI to automatically invoke functions
        OpenAIPromptExecutionSettings settings = new() { 
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        Console.WriteLine(await kernel.InvokePromptAsync(
            "How many days until Christmas? Explain your thinking.", new(settings)));

        // Example 4. Invoke the kernel with a prompt and allow the AI to automatically invoke functions that use enumerations
        Console.WriteLine(await kernel.InvokePromptAsync("Create a handy lime colored widget for me.", new(settings)));
        Console.WriteLine(await kernel.InvokePromptAsync("Create a beautiful scarlet colored widget for me.", new(settings)));
        Console.WriteLine(await kernel.InvokePromptAsync("Create an attractive maroon and navy colored widget for me.", new(settings)));
    }
}