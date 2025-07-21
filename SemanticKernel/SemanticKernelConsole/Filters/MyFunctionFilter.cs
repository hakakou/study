using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernelConsole.Filters;

public sealed class MyFunctionFilter(ILogger<MyFunctionFilter> logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        logger.LogInformation($"Invoking {context.Function.Name}");
        await next(context);

        var metadata = context.Result?.Metadata;
        if (metadata is not null && metadata.ContainsKey("Usage"))
        {
            logger.LogInformation($"Token usage: {metadata["Usage"]?.AsJson()}");
        }
    }
}
