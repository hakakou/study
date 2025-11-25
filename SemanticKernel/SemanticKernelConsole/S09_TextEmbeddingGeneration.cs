using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using SharedConfig;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

/*

public class S09_TextEmbeddingGeneration : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAITextEmbeddingGeneration(
                modelId: "text-embedding-3-small",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));

        // Choose a collection from the database and specify the type of key and record stored in it via Generic parameters.
        var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels");

        await collection.CreateCollectionIfNotExistsAsync();

        var description1 = "A hotel where everyone can be happy.";
        var description2 = "A tropical hotel.";

        var embeddings = await textEmbeddingGenerationService.GenerateEmbeddingsAsync([description1, description2]);
        
        await collection.UpsertAsync(new Hotel
        {
            HotelId = 1,
            HotelName = "Hotel Happy",
            Description = description1,
            DescriptionEmbedding = embeddings.First(),
            Tags = new[] { "luxury", "pool", "sunny" }
        });

        //embeddings = await textEmbeddingGenerationService.GenerateEmbeddingsAsync([description]);
        await collection.UpsertAsync(new Hotel
        {
            HotelId = 2,
            HotelName = "Hot Hotel",
            Description = description2,
            DescriptionEmbedding = embeddings.Last(),
            Tags = new[] { "Sauna", "pool" }
        });

        // search
        var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingsAsync(
            ["I want a warm hotel"]);

        var searchResult = await collection.VectorizedSearchAsync(searchVector.First(), new() { Top = 1 });
        var r = await searchResult.Results.FirstAsync();
        Console.WriteLine("Result: " + r.Record.HotelName);
    }

    public class Hotel
    {
        [VectorStoreRecordKey]
        public ulong HotelId { get; set; }

        [VectorStoreRecordData(IsFilterable = true)]
        public string HotelName { get; set; }

        [VectorStoreRecordData(IsFullTextSearchable = true)]
        public string Description { get; set; }

        //[VectorStoreRecordVector(Dimensions: 4, DistanceFunction.CosineSimilarity, IndexKind.Hnsw)]
        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

        [VectorStoreRecordData(IsFilterable = true)]
        public string[] Tags { get; set; }
    }
}

*/