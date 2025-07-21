using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelConsole.Filters;
using SemanticKernelConsole.Functions;
using Spectre.Console;

public class S105_Filters : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        builder.Plugins.AddFromType<TimePlugin>();
        builder.Services.AddSingleton<IFunctionInvocationFilter, MyFunctionFilter>();
        builder.Services.AddSingleton<IPromptRenderFilter, MyPromptFilter>();

        var kernel = builder.Build();

        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        var args = new KernelArguments(settings) { { "topic", "may day" } };

        var r = await kernel.InvokePromptAsync("How many days until {{$topic}} ?", arguments: args);

        AnsiConsole.MarkupLine($"[blue]{r}[/]");
    }
}