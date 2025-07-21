using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SharedConfig;
using System.ClientModel;

class T09_Rag
{
    private static EmbeddingClient embeddingClient;
    private static ChatClient chatClient;
    private static Dictionary<int, float[]> vectorStore = new();
    static string[] documents;

    public static async Task Run()
    {
        chatClient = Program.ChatClient;

        var azureClient = new AzureOpenAIClient(
                    new Uri(Conf.AzureAIFoundry.Endpoint),
                    new ApiKeyCredential(Conf.AzureAIFoundry.ApiKey));

        embeddingClient = azureClient.GetEmbeddingClient("text-embedding-3-large");

        // Step 1: Preprocessing

        documents = new string[] {
            "Coconut is the capital of Banana.",
            "Bilbao is the capital of Spain.",
            "Lapona is the capital of the country named Diginia."
        };

        EmbeddingGenerationOptions options = new() { Dimensions = 3072 };
        for (int i = 0; i < documents.Length; i++)
        {
            float[] embedding = (await embeddingClient.GenerateEmbeddingAsync(documents[i], options))
                .Value.ToFloats().ToArray();
            vectorStore[i] = embedding;
        }

        // Each document is now represented by a vector with 3072 dimensions

        Console.WriteLine("Data preprocessed and stored in vector database.");

        Console.WriteLine("RAG Console Application Started");
        while (true)
        {
            Console.Write("You: ");
            string userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput)) break;

            string response = await GetAnswerFromUserInput(userInput);
            Console.WriteLine($"Assistant: {response}");
        }
    }

    private static async Task<string> GetAnswerFromUserInput(string userInput)
    {
        // Step 2A: Calculate Embeddings for User Input
        float[] userEmbedding = (await embeddingClient.GenerateEmbeddingAsync(userInput)).Value.ToFloats().ToArray();

        // Step 2B: Retrieve N similar chunks (here, top-1 for simplicity)
        string mostRelevantDoc = RetrieveMostSimilarDocument(userEmbedding);

        // Step 2C: Generate a response using OpenAI Chat
        string systemPrompt = "You are a helpful assistant answering questions based on provided context.";
        List<ChatMessage> messages = new()
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Context: {mostRelevantDoc}\n\nQuestion: {userInput}")
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages,
            new ChatCompletionOptions { MaxOutputTokenCount = 150 });
        return completion.Content[0].Text;
    }

    private static string RetrieveMostSimilarDocument(float[] userEmbedding)
    {
        int? bestMatch = null;
        float bestScore = float.MinValue;

        foreach (var doc in vectorStore)
        {
            float score = CalculateCosineSimilarity(userEmbedding, doc.Value);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = doc.Key;
            }
        }

        return bestMatch != null ? documents[bestMatch.Value] : "No relevant document found.";
    }

    private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        float dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
        float magnitudeA = (float)Math.Sqrt(vectorA.Sum(a => a * a));
        float magnitudeB = (float)Math.Sqrt(vectorB.Sum(b => b * b));
        return dotProduct / (magnitudeA * magnitudeB);
    }
}
