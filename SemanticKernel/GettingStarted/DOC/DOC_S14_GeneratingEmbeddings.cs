using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

public class DOC_S14_GeneratingEmbeddings(
    IEmbeddingGenerator<string, Embedding<float>> embeddingClient) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultEmbeddings();
    }

    public async Task Run()
    {
        await Example1_VectorStoreLevel();
        await Example2_CollectionLevel();
        await Example3_RecordDefinitionLevel();
    }

    // Example 1: Set embedding generator at the Vector Store level
    // This generator will be used for all collections and properties unless overridden.
    private async Task Example1_VectorStoreLevel()
    {
        Console.WriteLine("\n=== Example 1: Vector Store Level ===");

        var vectorStore = new QdrantVectorStore(
            new QdrantClient("localhost"),
            ownsClient: true,
            new QdrantVectorStoreOptions
            {
                EmbeddingGenerator = embeddingClient
            });

        var collection = vectorStore.GetCollection<ulong, FinanceInfo>("finance_store");
        await collection.EnsureCollectionExistsAsync();

        var record = new FinanceInfo
        {
            Key = 1,
            Text = "Revenue increased by 25% year-over-year"
        };

        await collection.UpsertAsync(record);
        Console.WriteLine("Record upserted with store-level embedding generator");
    }

    // Example 2: Set embedding generator at the Collection level
    // This overrides the store-level generator for this specific collection.
    private async Task Example2_CollectionLevel()
    {
        Console.WriteLine("\n=== Example 2: Collection Level ===");

        var collectionOptions = new QdrantCollectionOptions
        {
            EmbeddingGenerator = embeddingClient
        };
        var collection = new QdrantCollection<ulong, FinanceInfo>(
            new QdrantClient("localhost"),
            "finance_store",
            ownsClient: true,
            collectionOptions);

        await collection.EnsureCollectionExistsAsync();

        var record = new FinanceInfo
        {
            Key = 2,
            Text = "Operating expenses decreased by 10%"
        };

        await collection.UpsertAsync(record);
        Console.WriteLine("Record upserted with collection-level embedding generator");
    }

    // Example 3: Set embedding generator at the Record Definition level
    // This applies to all vector properties in the definition.
    private async Task Example3_RecordDefinitionLevel()
    {
        Console.WriteLine("\n=== Example 3: Record Definition Level ===");

        var definition = new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = embeddingClient,
            Properties = new List<VectorStoreProperty>
            {
                new VectorStoreKeyProperty("Key", typeof(ulong)),
                new VectorStoreDataProperty("Text", typeof(string)),
                new VectorStoreVectorProperty("Embedding", typeof(string), dimensions: 1536)
            }
        };

        var collectionOptions = new QdrantCollectionOptions
        {
            Definition = definition
        };

        var collection = new QdrantCollection<ulong, FinanceInfo>(
            new QdrantClient("localhost"),
            "finance_store",
            ownsClient: true,
            collectionOptions);

        await collection.EnsureCollectionExistsAsync();

        await collection.UpsertAsync(new FinanceInfo
        {
            Key = 3,
            Text = "Net income margin improved to 15%"
        });
        Console.WriteLine("Record upserted with definition-level embedding generator");
    }

    internal class FinanceInfo
    {
        [VectorStoreKey]
        public ulong Key { get; set; }

        [VectorStoreData]
        public string Text { get; set; } = string.Empty;

        // Note that the vector property is typed as a string, and
        // its value is derived from the Text property. The string
        // value will however be converted to a vector on upsert and
        // stored in the database as a vector.
        [VectorStoreVector(Conf.DefaultEmbeddingDimension)]
        public string Embedding => this.Text;
    }
}