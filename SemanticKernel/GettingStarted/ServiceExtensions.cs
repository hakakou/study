using Anthropic.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0050

public static class ServiceExtensions
{
    public static IKernelBuilder DefaultChatCompletion(this IKernelBuilder services)
    {
        return services.AddAzureOpenAIChatCompletion(
                    deploymentName: Conf.AzureFoundry.DeploymentName,
                    endpoint: Conf.AzureFoundry.Endpoint,
                    apiKey: Conf.AzureFoundry.ApiKey
                );
    }

    public static IKernelBuilder DefaultEmbeddings(this IKernelBuilder services)
    {
        return services.AddAzureOpenAIEmbeddingGenerator(
                    deploymentName: Conf.AzureFoundryEmbeddings.DeploymentName, // 3072
                    endpoint: Conf.AzureFoundryEmbeddings.Endpoint,
                    apiKey: Conf.AzureFoundryEmbeddings.ApiKey);
    }

    public static IKernelBuilder GoogleTextSearch(this IKernelBuilder services)
    {
        return services.AddGoogleTextSearch(
                    searchEngineId: Conf.GoogleTextSearch.SearchEngineId,
                    apiKey: Conf.GoogleTextSearch.ApiKey);
    }

    public static IKernelBuilder BingTextSearch(this IKernelBuilder services)
    {
        return services.AddBingTextSearch(
                    apiKey: Conf.BingTextSearch.ApiKey);
    }

    public static IKernelBuilder DefaultAnthropic(this IKernelBuilder services)
    {
        var client = new AnthropicClient(new APIAuthentication(Conf.AzureAnthropic.ApiKey))
        {
            ApiUrlFormat = Conf.AzureAnthropic.Endpoint + "/{0}/{1}",
        };

        var chatClient = new ChatClientBuilder(client.Messages)
            .UseFunctionInvocation()
            .Build();

        services.Services.AddSingleton<IChatClient>(chatClient);
        services.Services.AddSingleton<IChatCompletionService>(chatClient.AsChatCompletionService());

        return services;
    }
}