using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050

// https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/assistant-agent
public static class ServiceExtensions
{
    public static IKernelBuilder DefaultChatCompletion(this IKernelBuilder services)
    {
        Console.WriteLine($"Model: {Conf.AzureFoundry.DeploymentName}");

        return services.AddAzureOpenAIChatCompletion(
                    deploymentName: Conf.AzureFoundry.DeploymentName,
                    endpoint: Conf.AzureFoundry.Endpoint,
                    apiKey: Conf.AzureFoundry.ApiKey
                );
    }

    // ITextSearch

    public static IKernelBuilder GoogleTextSearch(this IKernelBuilder services)
    {
        return services.AddGoogleTextSearch(
                    searchEngineId: Conf.GoogleTextSearch.SearchEngineId,
                    apiKey: Conf.GoogleTextSearch.ApiKey);
    }

    public static IKernelBuilder TavilyTextSearch(this IKernelBuilder services)
    {
        return services.AddTavilyTextSearch(
                    apiKey: Conf.TavilyTextSearch.ApiKey);
    }

}