using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SharedConfig;
using System.ClientModel;

internal class T10_Rag_Enhanced
{
    private static EmbeddingClient embeddingClient;
    private static ChatClient chatClient;
    private static Dictionary<int, float[]> vectorStore = new();
    private static string[] documents;

    public static async Task Run()
    {
        chatClient = Program.ChatClient;

        var azureClient = new AzureOpenAIClient(
                    new Uri(Conf.AzureFoundry.Endpoint),
                    new ApiKeyCredential(Conf.AzureFoundry.ApiKey));

        embeddingClient = azureClient.GetEmbeddingClient("text-embedding-3-large");

        // Step 1: Preprocessing
        documents = new string[] {
            "The Eiffel Tower is located in Paris, France, and stands as a symbol of French culture and engineering.",
            "Albert Einstein is best known for developing the theory of relativity, a cornerstone of modern physics.",
            "Photosynthesis is the process by which green plants and some organisms use sunlight to synthesize nutrients from carbon dioxide and water.",
            "The Great Wall of China is a historic series of walls and fortifications built to protect Chinese states from invasions.",
            "JavaScript is a programming language commonly used for web development, allowing developers to create interactive elements on websites.",
            "Shakespeare's 'Romeo and Juliet' is a classic tragedy that explores themes of love, family conflict, and fate.",
            "The Amazon rainforest, often called the 'lungs of the Earth,' is a critical biome for global biodiversity and carbon sequestration.",
            "Blockchain technology underpins cryptocurrencies like Bitcoin and ensures secure, transparent, and tamper-proof transactions.",
            "In mathematics, the Pythagorean theorem relates the lengths of the sides of a right triangle: a² + b² = c².",
            "The concept of 'survival of the fittest' originates from Darwin's theory of natural selection, a key principle in evolutionary biology."
        };

        EmbeddingGenerationOptions options = new() { Dimensions = 3072 };
        for (int i = 0; i < documents.Length; i++)
        {
            float[] embedding = (await embeddingClient.GenerateEmbeddingAsync(documents[i], options))
                .Value.ToFloats().ToArray();
            vectorStore[i] = embedding;
        }

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
        // Step 2A: Generate System Prompt
        string systemPrompt = GenerateSystemPrompt(userInput);
        Console.WriteLine($"Generated System Prompt: {systemPrompt}");

        // Step 2B: Calculate Embeddings for User Input
        float[] userEmbedding = (await embeddingClient.GenerateEmbeddingAsync(userInput)).Value.ToFloats().ToArray();

        // Step 2C: Retrieve N similar chunks (here, top-1 for simplicity)
        string mostRelevantDoc = RetrieveMostSimilarDocument(userEmbedding);

        // Step 2D: Generate a response using OpenAI Chat
        string assistantPrompt = $"{systemPrompt}\n\nContext: {mostRelevantDoc}\n\nQuestion: {userInput}";
        List<ChatMessage> messages = new()
        {
            new SystemChatMessage("You are a helpful assistant answering questions based on provided context."),
            new UserChatMessage(assistantPrompt)
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages,
            new ChatCompletionOptions { MaxOutputTokenCount = 150 });
        return completion.Content[0].Text;
    }

    private static string GenerateSystemPrompt(string userInput)
    {
        return $"Given the chat history and user question, generate a search query that will return the best answer from the knowledge base. " +
               $"Do NOT use quotes or other search operators. Avoid filenames or document names in the search query terms. " +
               $"Translate non-English questions to English. Search query: {userInput}";
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