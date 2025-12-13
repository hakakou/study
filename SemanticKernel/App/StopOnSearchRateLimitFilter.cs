using System.Net;
using Microsoft.SemanticKernel;

public sealed class StopOnSearchRateLimitFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (HttpOperationException ex)
            when (ex.StatusCode == (HttpStatusCode)432 || ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            // Stop any further auto tool calling in this prompt execution
            context.Terminate = true;
            throw;
        }
    }
}
