using Microsoft.Extensions.DependencyInjection;

public class DOC_S38_AzureOpenAIAgent : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {
    }
}