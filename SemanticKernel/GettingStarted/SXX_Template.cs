using Microsoft.Extensions.DependencyInjection;

public class SXX_Template : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {
    }
}