using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using SharedConfig;
using System.ComponentModel;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/*

public class S306_Search_With_VectorStore : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIEmbeddingGenerator(
                modelId: "text-embedding-3-small",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();


        var textEmbeddingGeneration = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        var inMemoryVectorStore = new InMemoryVectorStore();
        var collection = inMemoryVectorStore.GetCollection<Guid, DataModel>("records");
        await collection.CreateCollectionIfNotExistsAsync();

        // Create records and generate embeddings for them.

        foreach (var line in lines)
        {
            var embedding = await textEmbeddingGeneration.GenerateEmbeddingAsync(line);

            var guid = Guid.NewGuid();
            var record = new DataModel()
            {
                Key = guid,
                Text = line,
                Link = $"guid://{guid}",
                Tag = guid.ToByteArray()[0] % 2 == 0 ? "Even" : "Odd",
                Embedding = embedding
            };
            await collection.UpsertAsync(record);
        }

        // Create a text search instance using the InMemory vector store.
        var textSearch = new VectorStoreTextSearch<DataModel>(collection, textEmbeddingGeneration);

        // Search and return results as TextSearchResult items
        var query = "Ποιοι έχουν αγγλικά για ξένη γλώσσα και είναι τεκνο πολύτεκνης οικογένειας;";

        // 1st way: Get text search results
        //
        //KernelSearchResults<TextSearchResult> textResults =
        //    await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });
        //
        //await foreach (TextSearchResult result in textResults.Results)
        //{
        //    Console.WriteLine($"Name:  {result.Name}");
        //    Console.WriteLine($"Value: {result.Value}");
        //    Console.WriteLine($"Link:  {result.Link}");
        //}

        // 2nd way: Get vector search results

        //KernelSearchResults<object> searchResults =
        //    await textSearch.GetSearchResultsAsync(query, new() { Top = 1, Skip = 0, IncludeTotalCount = true });

        //await foreach (DataModel result in searchResults.Results)
        //    Console.WriteLine($"Name:  {result.Text}");

        // 3rd way: Get vector search results with filters
        var searchVector = await textEmbeddingGeneration.GenerateEmbeddingsAsync([query]);
        var searchResults = await collection.VectorizedSearchAsync(searchVector.First(), new() { Top = 3 });

        await foreach (VectorSearchResult<DataModel> r in searchResults.Results)
        {
            Console.WriteLine("Result: " + r.Record.Text);
            Console.WriteLine("Score: " + r.Score);
        }
    }

    string[] lines =
    [
        // ...existing code...
    ];

    public sealed class DataModel
    {
        [VectorStoreRecordKey]
        [TextSearchResultName]
        public Guid Key { get; init; }

        [VectorStoreRecordData]
        [TextSearchResultValue]
        public string Text { get; init; }

        [VectorStoreRecordData]
        [TextSearchResultLink]
        public string Link { get; init; }

        [VectorStoreRecordData(IsFilterable = true)]
        public required string Tag { get; init; }

        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> Embedding { get; init; }
    }
}
*/