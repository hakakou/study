using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelConsole.Filters;

public sealed class MyPromptFilter(ILogger<MyPromptFilter> logger) : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context,
        Func<PromptRenderContext, Task> next)
    {
        logger.LogInformation($"Rendering prompt: {context.Function.Name}");

        if (context.Arguments.ContainsName("topic"))
        {
            if (context.Arguments["topic"] == "chocolate")
                context.Arguments["topic"] = "****";
        }

        await next(context);
        logger.LogInformation($"Prompt rendered: {context.RenderedPrompt}");
    }
}