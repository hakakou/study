using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OpenAI.Embeddings;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

[RunDirectlyAttribute]
public class DOC_S10_TextEmbeddingGeneration : ITest
{
    public async Task Run()
    {
        await AddDirectlyToKernelExample();
        await StandaloneInstanceExample();
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
                model: "text-embedding-3-small",
                apiKey: Conf.OpenAI.ApiKey);

        var embedding =
            (await embeddingGenerator.GenerateEmbeddingAsync("Hello, Semantic Kernel!")).Value;

        Console.WriteLine($"Generated embedding with {embedding.ToFloats().Length} dimensions");
        Console.WriteLine(
            $"First 5 values: {string.Join(", ", embedding.ToFloats()[..5].ToArray().Select(v => v.ToString("F4")))}");
    }
}