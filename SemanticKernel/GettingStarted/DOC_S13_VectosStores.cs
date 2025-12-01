using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

public class DOC_S13_VectosStores(
    IEmbeddingGenerator<string, Embedding<float>> embeddingClient) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultEmbeddings();
    }

    public async Task Run()
    {
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true,
        new QdrantVectorStoreOptions
        {
            EmbeddingGenerator = embeddingClient
        });

        // Choose a collection from the database and specify the type of key and record stored in it via Generic parameters.
        var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels2");

        await SearchWithFilter(collection, "I want a relaxing vacation", []);

        await SearchWithFilter(collection, "I want a relaxing vacation", ["pets"]);

        await HybridSearch(collection, "I want a relaxing vacation", []);
        await HybridSearch(collection, "I want a relaxing vacation", ["dog"]);

        //await HybridSearch(collection, "I'm looking for a hotel where customer happiness is the priority.",
        //    ["happiness", "hotel", "customer"]);
    }

    private async Task SearchWithFilter(QdrantCollection<ulong, Hotel> collection,
        string query, string[] tags)
    {
        Console.WriteLine($"\nSearching for: '{query}'");
        Console.WriteLine($"Keywords: {string.Join(", ", tags)}");
        Console.WriteLine(new string('-', 60));

        var searchEmbedding = await embeddingClient.GenerateAsync(query);

        VectorSearchOptions<Hotel> vectorSearchOptions = null;
        if (tags.Length > 0)
        {
            var tag0 = tags[0];
            vectorSearchOptions = new VectorSearchOptions<Hotel>
            {
                Filter = r => r.Tags.Contains(tag0)
            };
        }

        // Perform the search with the filter
        var searchResult = collection.SearchAsync(searchEmbedding, top: 3, vectorSearchOptions);

        // Iterate over the search results
        await foreach (var result in searchResult)
        {
            Console.WriteLine($"Hotel: {result.Record.HotelName}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Tags: {string.Join(", ", result.Record.Tags)}");
            Console.WriteLine($"Score: {result.Score:F4}");
            Console.WriteLine();
        }
    }

    private async Task HybridSearch(QdrantCollection<ulong, Hotel> collection,
        string query, string[] keywords)
    {
        // Hybrid search combines vector similarity search with keyword search
        // This provides better results by leveraging both semantic understanding and exact keyword matches

        Console.WriteLine($"\nSearching for: '{query}'");
        Console.WriteLine($"Keywords: {string.Join(", ", keywords)}");
        Console.WriteLine(new string('-', 60));

        // Generate a vector for the search text
        var searchEmbedding = await embeddingClient.GenerateAsync(query);

        // Cast the collection to IKeywordHybridSearchable to access hybrid search functionality
        var hybridSearchableCollection = collection as IKeywordHybridSearchable<Hotel>;

        // Perform hybrid search: combines vector similarity with keyword matching
        // The keywords help find documents that contain specific terms
        // while the vector ensures semantic relevance
        var searchResult = hybridSearchableCollection.HybridSearchAsync(
            searchEmbedding.Vector,
            keywords,
            top: 3);

        // Iterate over the search results
        await foreach (var result in searchResult)
        {
            Console.WriteLine($"Hotel: {result.Record.HotelName}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Tags: {string.Join(", ", result.Record.Tags)}");
            Console.WriteLine($"Score: {result.Score:F4}");
            Console.WriteLine();
        }
    }

    private async Task Upsert()
    {
        Utils.PrintSectionHeader("Connect to Database and Manage Collections");

        // Create a Qdrant VectorStore object
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true,
        new QdrantVectorStoreOptions
        {
            EmbeddingGenerator = embeddingClient
        });

        // Choose a collection from the database and specify the type of key and record stored in it via Generic parameters.
        var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels2");

        // Create the collection if it doesn't exist yet.
        await collection.EnsureCollectionExistsAsync();

        // Upsert 20 hotel records
        await UpsertHotelExamplesAsync(collection);

        // Retrieve the upserted record
        Hotel? retrievedHotel = await collection.GetAsync(3);
        if (retrievedHotel != null)
        {
            Console.WriteLine($"Retrieved hotel: {retrievedHotel.HotelName}");
            Console.WriteLine($"Description: {retrievedHotel.Description}");
            Console.WriteLine($"Tags: {string.Join(", ", retrievedHotel.Tags)}");
        }
    }

    private async Task UpsertHotelExamplesAsync(VectorStoreCollection<ulong, Hotel> collection)
    {
        var hotels = new[]
        {
            new { Id = 1UL, Name = "Hotel Happy", Description = "A place where everyone can be happy.", Tags = new[] { "luxury", "pool" } },
            new { Id = 2UL, Name = "Grand Plaza Hotel", Description = "Elegant accommodations in the heart of the city with exceptional service.", Tags = new[] { "luxury", "business", "downtown" } },
            new { Id = 3UL, Name = "Seaside Resort", Description = "Beautiful beachfront property with stunning ocean views and water sports.", Tags = new[] { "beach", "family-friendly", "pool" } },
            new { Id = 4UL, Name = "Mountain Lodge", Description = "Cozy retreat in the mountains perfect for skiing and hiking adventures.", Tags = new[] { "mountain", "skiing", "nature" } },
            new { Id = 5UL, Name = "Budget Inn", Description = "Affordable and clean rooms for travelers on a budget.", Tags = new[] { "budget", "convenient" } },
            new { Id = 6UL, Name = "Boutique Luxury Suites", Description = "Exclusive boutique hotel with personalized service and unique design.", Tags = new[] { "luxury", "boutique", "romantic" } },
            new { Id = 7UL, Name = "Airport Express Hotel", Description = "Convenient location near the airport with free shuttle service.", Tags = new[] { "airport", "business", "convenient" } },
            new { Id = 8UL, Name = "Historic Heritage Inn", Description = "Charming historic building with period features and modern amenities.", Tags = new[] { "historic", "romantic", "downtown" } },
            new { Id = 9UL, Name = "Family Fun Resort", Description = "All-inclusive resort with kids club, water park, and family activities.", Tags = new[] { "family-friendly", "pool", "all-inclusive" } },
            new { Id = 10UL, Name = "Spa & Wellness Retreat", Description = "Luxurious spa resort focused on relaxation and holistic wellness.", Tags = new[] { "spa", "luxury", "wellness" } },
            new { Id = 11UL, Name = "Business Center Hotel", Description = "Modern hotel with conference rooms and business amenities. No kids allowed.", Tags = new[] { "business", "downtown", "conference" } },
            new { Id = 12UL, Name = "Eco Lodge", Description = "Sustainable accommodations in harmony with nature and the environment.", Tags = new[] { "eco-friendly", "nature", "sustainable" } },
            new { Id = 13UL, Name = "Casino Resort", Description = "Exciting casino hotel with entertainment, gaming, and nightlife.", Tags = new[] { "casino", "entertainment", "nightlife" } },
            new { Id = 14UL, Name = "Pet-Friendly Inn", Description = "Welcoming hotel that treats your pets like family members.", Tags = new[] { "pet-friendly", "family-friendly" } },
            new { Id = 15UL, Name = "Golf Resort", Description = "Championship golf course with luxury accommodations for golf enthusiasts.", Tags = new[] { "golf", "luxury", "sports" } },
            new { Id = 16UL, Name = "Urban Hostel", Description = "Social hostel with shared spaces perfect for meeting fellow travelers.", Tags = new[] { "budget", "social", "downtown" } },
            new { Id = 17UL, Name = "Vineyard Hotel", Description = "Charming hotel surrounded by vineyards with wine tasting experiences.", Tags = new[] { "wine", "romantic", "nature" } },
            new { Id = 18UL, Name = "Ski Chalet", Description = "Slope-side accommodations with direct access to ski lifts.", Tags = new[] { "skiing", "mountain", "luxury" } },
            new { Id = 19UL, Name = "Island Paradise Resort", Description = "Tropical island resort with pristine beaches and crystal clear waters.", Tags = new[] { "beach", "luxury", "tropical" } },
            new { Id = 20UL, Name = "Downtown Lofts", Description = "Modern loft-style rooms in a converted warehouse in the arts district.", Tags = new[] { "downtown", "boutique", "trendy" } }
        };

        foreach (var hotel in hotels)
        {
            var embedding = await embeddingClient.GenerateAsync(hotel.Description
                + ". Tags: " + string.Join(", ", hotel.Tags));
            await collection.UpsertAsync(new Hotel
            {
                HotelId = hotel.Id,
                HotelName = hotel.Name,
                Description = hotel.Description,
                DescriptionEmbedding = embedding.Vector,
                Tags = hotel.Tags
            });

            Console.WriteLine($"Upserted hotel: {hotel.Id} - {hotel.Name}");
        }
    }

    public class Hotel
    {
        [VectorStoreKey]
        public ulong HotelId { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string HotelName { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Description { get; set; }

        [VectorStoreVector(Dimensions: 3072, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
        public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string[] Tags { get; set; }
    }
}