using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Functions;
using System.ComponentModel;

public class DOC_S33_AgentContextualFunctions(
    Kernel kernel,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion()
            .DefaultEmbeddings();

        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        Utils.PrintSectionHeader("Contextual Function Selection with Agents");

        Console.WriteLine("Contextual Function Selection uses RAG to dynamically select and advertise");
        Console.WriteLine("only the most relevant functions based on the current conversation context.\n");

        // Create a chat completion agent with contextual function selection
        ChatCompletionAgent agent = new()
        {
            Name = "ReviewGuru",
            Instructions = "You are a friendly assistant that summarizes key points and sentiments from customer reviews. For each response, list the functions you used.",
            Kernel = kernel,
            Arguments = new(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                    options: new FunctionChoiceBehaviorOptions { AllowConcurrentInvocation = true })
            }),
            // IMPORTANT: This setting must be set to true when using the ContextualFunctionProvider
            UseImmutableKernel = true
        };

        // Create the agent thread
        ChatHistoryAgentThread agentThread = new();

        // Register the contextual function provider
        agentThread.AIContextProviders.Add(
            new ContextualFunctionProvider(
                vectorStore: new InMemoryVectorStore(
                    new InMemoryVectorStoreOptions()
                    {
                        EmbeddingGenerator = embeddingGenerator
                    }),
                vectorDimensions: 3072, // Dimensions for text-embedding-3-large
                functions: GetAvailableFunctions(),
                maxNumberOfFunctions: 3, // Only the top 3 relevant functions are advertised
                options: new ContextualFunctionProviderOptions
                {
                    NumberOfRecentMessagesInContext = 2
                }
            )
        );

        try
        {
            // Example 1: Basic invocation - only relevant functions will be selected
            Console.WriteLine("\n[Example 1] Agent with contextual function selection:\n");
            await foreach (var response in agent.InvokeAsync("Get and summarize customer reviews.", agentThread))
            {
                response.Message.PrintChatMessageContent();
            }

            // Example 2: Follow-up question using conversation context
            Console.WriteLine("\n[Example 2] Follow-up using conversation context:\n");
            await foreach (var response in agent.InvokeAsync("What's the overall sentiment?", agentThread))
            {
                response.Message.PrintChatMessageContent();
            }

            // Example 3: Irrelevant query - should select different functions
            Console.WriteLine("\n[Example 3] Different context selects different functions:\n");
            await foreach (var response in agent.InvokeAsync("What's the current weather?", agentThread))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await agentThread.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));

        // Demonstrate custom context embedding provider
        //await DemonstrateCustomContextEmbedding();
    }

    /// <summary>
    /// Demonstrates customizing the context embedding value provider
    /// </summary>
    private async Task DemonstrateCustomContextEmbedding()
    {
        Utils.PrintSectionHeader("Custom Context Embedding Provider");

        ChatCompletionAgent agent = new()
        {
            Name = "CustomContextAgent",
            Instructions = "You are a helpful assistant. List the functions you used in your response.",
            Kernel = kernel,
            Arguments = new(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }),
            UseImmutableKernel = true
        };

        ChatHistoryAgentThread agentThread = new();

        agentThread.AIContextProviders.Add(
            new ContextualFunctionProvider(
                vectorStore: new InMemoryVectorStore(
                    new InMemoryVectorStoreOptions()
                    {
                        EmbeddingGenerator = embeddingGenerator
                    }),
                vectorDimensions: 3072,
                functions: GetAvailableFunctions(),
                maxNumberOfFunctions: 2,
                options: new ContextualFunctionProviderOptions
                {
                    // Custom provider that only uses user messages for context embedding
                    ContextEmbeddingValueProvider = async (recentMessages, newMessages, cancellationToken) =>
                    {
                        var allUserMessages = recentMessages.Concat(newMessages)
                            .Where(m => m.Role == ChatRole.User)
                            .Select(m => m.Contents)
                            .Where(content => !string.IsNullOrWhiteSpace(content.ToString()));

                        var contextValue = string.Join("\n", allUserMessages);
                        Console.WriteLine($"[Custom Context] Embedding value: {contextValue}\n");
                        return contextValue;
                    }
                }
            )
        );

        try
        {
            await foreach (var response in agent.InvokeAsync("Tell me about customer reviews", agentThread))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await agentThread.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));
    }

    /// <summary>
    /// Returns a list of available functions - both relevant and irrelevant
    /// to demonstrate the benefits of contextual filtering
    /// </summary>
    private IReadOnlyList<AIFunction> GetAvailableFunctions()
    {
        return new List<AIFunction>
        {
            // Relevant functions for customer review analysis
            AIFunctionFactory.Create(
                () => """
                [
                    { "reviewer": "John D.", "date": "2023-10-01", "rating": 5, "comment": "Great product and fast shipping!" },
                    { "reviewer": "Sarah M.", "date": "2023-10-02", "rating": 4, "comment": "Good quality, but a bit pricey." },
                    { "reviewer": "Mike R.", "date": "2023-10-03", "rating": 5, "comment": "Excellent customer service and product!" }
                ]
                """,
                "GetCustomerReviews",
                "Retrieves a list of customer reviews with ratings and comments"),

            AIFunctionFactory.Create(
                (string text) => $"Summary: The data shows positive customer feedback with an average rating of 4.7/5. Key themes include product quality, fast delivery, and good customer service.",
                "Summarize",
                "Generates a comprehensive summary based on input data"),

            AIFunctionFactory.Create(
                (string text) => "Sentiment Analysis: Predominantly positive (85%), with minor concerns about pricing. Overall satisfaction is high.",
                "CollectSentiments",
                "Analyzes and collects sentiment information from text data"),

            AIFunctionFactory.Create(
                (string reviewText) => $"Analysis of '{reviewText}': This review appears genuine based on language patterns and specificity of details.",
                "AnalyzeReviewAuthenticity",
                "Determines if a customer review is authentic or potentially fake"),

            // Irrelevant functions (should not be selected for review-related queries)
            AIFunctionFactory.Create(
                () => "Current weather: Sunny, 72°F with light winds.",
                "GetWeather",
                "Retrieves the current weather conditions"),

            AIFunctionFactory.Create(
                (string recipient, string subject, string body) => $"Email sent to {recipient} with subject '{subject}'",
                "SendEmail",
                "Sends an email to a specified recipient"),

            AIFunctionFactory.Create(
                (string symbol) => $"Current stock price for {symbol}: $123.45 (up 2.3% today)",
                "GetStockPrice",
                "Gets the current stock price for a given symbol"),

            AIFunctionFactory.Create(
                () => $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                "GetCurrentTime",
                "Returns the current system time"),

            AIFunctionFactory.Create(
                (string query) => $"Search results for '{query}': [Multiple web results would appear here]",
                "WebSearch",
                "Performs a web search and returns results"),

            AIFunctionFactory.Create(
                (int a, int b) => $"The result of {a} + {b} = {a + b}",
                "Calculate",
                "Performs basic arithmetic calculations")
        };
    }
}