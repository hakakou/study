using Anthropic.SDK;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using System.ClientModel;
using System.Net.Http.Headers;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050

// https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/assistant-agent
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

    public static IKernelBuilder DefaultOpenAIAssistantClient(this IKernelBuilder services)
    {
        // var client = OpenAIAssistantAgent.CreateOpenAIClient(new ApiKeyCredential(Conf.OpenAI.ApiKey));

        var clientAzure = OpenAIAssistantAgent.CreateAzureOpenAIClient(
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey),
            new Uri(Conf.AzureFoundry.Endpoint));

        AssistantClient assistant = clientAzure.GetAssistantClient();
        services.Services.AddSingleton(assistant);
        return services;
    }

    public static IKernelBuilder DefaultChatClient(this IKernelBuilder services)
    {
        IChatClient clientAzure = new AzureOpenAIClient(
            new Uri(Conf.AzureFoundry.Endpoint),
            new ApiKeyCredential(Conf.AzureFoundry.ApiKey))
            .GetChatClient(Conf.AzureFoundry.DeploymentName)
            .AsIChatClient();

        services.Services.AddSingleton<IChatClient>(clientAzure);
        return services;
    }

    public static void DefaultMem0Provider(this IServiceCollection services)
    {
        services.AddHttpClient(name: "mem0", configureClient: options =>
        {
            options.BaseAddress = new Uri("https://api.mem0.ai");
            options.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", Conf.Mem0.ApiKey);
        });
    }

    public static IKernelBuilder DefaultChatCompletionOpenAI(this IKernelBuilder services)
    {
        return services.AddOpenAIChatCompletion(
                    modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey
                );
    }

    public static IKernelBuilder LocalChatCompletion(this IKernelBuilder services)
    {
        return services.AddOpenAIChatCompletion(
                    //modelId: "deepseek/deepseek-r1-0528-qwen3-8b",
                    modelId: "openai/gpt-oss-20b",
                    endpoint: new Uri("http://127.0.0.1:1234/v1"),
                    apiKey: ""
                );
    }
    public static IKernelBuilder DefaultEmbeddings(this IKernelBuilder services)
    {
        return services.AddAzureOpenAIEmbeddingGenerator(
                    deploymentName: Conf.AzureFoundryEmbeddings.DeploymentName, // 3072
                    endpoint: Conf.AzureFoundryEmbeddings.Endpoint,
                    apiKey: Conf.AzureFoundryEmbeddings.ApiKey);
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


    public static IKernelBuilder BingTextSearch(this IKernelBuilder services)
    {
        // Grounding with Bing Search: Very expensive, $35 / 1000 transactions
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