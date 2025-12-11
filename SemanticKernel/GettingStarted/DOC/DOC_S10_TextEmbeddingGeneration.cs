using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OpenAI.Embeddings;
using System.ClientModel;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

public class DOC_S10_TextEmbeddingGeneration : ITest
{
    public async Task Run()
    {
        await AddDirectlyToKernelExample();
        // await GenerateEmbeddings();
        // await StandaloneInstanceExample();
    }

    /// <summary>
    /// Example 1: Adding text embedding generation service directly to the kernel
    /// </summary>
    private async Task AddDirectlyToKernelExample()
    {
        Console.WriteLine("--- Example 1: Adding Directly to Kernel ---");

        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.AddOpenAIEmbeddingGenerator(
            modelId: "text-embedding-3-small",
            apiKey: Conf.OpenAI.ApiKey
        );
        Kernel kernel = kernelBuilder.Build();

        // Get the service from the kernel
        var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        // Generate embedding for a single text
        ReadOnlyMemory<float> embedding =
            await embeddingService.GenerateVectorAsync("Hello, Semantic Kernel!");

        Console.WriteLine($"Generated embedding with {embedding.Length} dimensions");
        Console.WriteLine($"First 5 values: {string.Join(", ", embedding.Slice(0, 5).ToArray().Select(v => v.ToString("F4")))}\n");

        Console.WriteLine();
    }

    /// <summary>
    /// Example 2: Creating standalone instance of the service
    /// </summary>
    private async Task StandaloneInstanceExample()
    {
        Console.WriteLine("--- Example 2: Standalone Instance ---");

        var embeddingGenerator =
            new EmbeddingClient(
                model: "text-embedding-3-large",
                apiKey: Conf.OpenAI.ApiKey);

        var embedding =
            (await embeddingGenerator.GenerateEmbeddingAsync("Hello, Semantic Kernel!")).Value;

        Console.WriteLine($"Generated embedding with {embedding.ToFloats().Length} dimensions");
        Console.WriteLine(
            $"First 5 values: {string.Join(", ", embedding.ToFloats()[..5].ToArray().Select(v => v.ToString("F4")))}");
    }


    async Task GenerateEmbeddings()
    {
        var azureClient = new AzureOpenAIClient(
                new Uri(Conf.AzureFoundryEmbeddings.Endpoint),
                new ApiKeyCredential(Conf.AzureFoundryEmbeddings.ApiKey));

        var embeddingClient = azureClient.GetEmbeddingClient(Conf.AzureFoundryEmbeddings.DeploymentName);

        // Get embeddings for the sentences
        var sentence1 = await embeddingClient.GenerateEmbeddingAsync("She works in tech since 2010, after graduating");
        var sentence2 = await embeddingClient.GenerateEmbeddingAsync("Verify inputs don't exceed the maximum length");

        // Calculate the dot product of the embeddings
        double dot = 0.0;
        var floats1 = sentence1.Value.ToFloats().ToArray();
        var floats2 = sentence2.Value.ToFloats().ToArray();
        for (int n = 0; n < floats1.Length; n++)
        {
            dot += floats1[0] * floats2[0];
        }

        Console.WriteLine($"Dot product: {dot}");
    }
}