using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

public class DOC_S35_AgentMemoryMem0(
    Kernel kernel, IHttpClientFactory httpClientFactory) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion();

        services.DefaultMem0Provider();

        services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        Utils.PrintSectionHeader("Using Mem0 for Agent Memory");

        Console.WriteLine("Mem0 is a self-improving memory layer for LLM applications,");
        Console.WriteLine("enabling personalized AI experiences with long-term memory.\n");

        //await Example1_BasicMem0UsageAsync();
        //await Example2_UserScopedMemoriesAsync();
        await Example3_ThreadScopedMemoriesAsync();
    }

    /// <summary>
    /// Basic example of using Mem0 with an agent
    /// </summary>
    private async Task Example1_BasicMem0UsageAsync()
    {
        Utils.PrintSectionHeader("Example 1: Basic Mem0 Memory Usage");

        // Create an HttpClient for the Mem0 service
        var httpClient = httpClientFactory.CreateClient("mem0");

        // Create a Mem0 provider for the current user
        var mem0Provider = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U1",
        });

        // Clear any previous memories (optional - useful for testing)
        // await mem0Provider.ClearStoredMemoriesAsync();

        // Create the agent
        ChatCompletionAgent agent = new()
        {
            Name = "MemoryAssistant",
            Instructions = "You are a helpful assistant that remembers user preferences and context. " +
              "Use the provided memories to personalize your responses.",
            Kernel = kernel,
            Arguments = new(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            })
        };

        // Create the agent thread and add the Mem0 provider
        ChatHistoryAgentThread agentThread = new();
        agentThread.AIContextProviders.Add(mem0Provider);

        try
        {
            // First conversation - agent learns about the user
            Console.WriteLine("[Conversation 1] Teaching the agent about preferences:\n");

            // Comment out the following line to see how the agent responds without memory
            {
                await foreach (var response in agent.InvokeAsync(
                    "My name is Alex and I prefer answers without bullet points, up to 3 sentences. I work in software development.",
                    agentThread))
                {
                    response.Message.PrintChatMessageContent();
                }
            }

            // Second conversation - agent recalls the information
            Console.WriteLine("\n[Conversation 2] Agent recalls user preferences:\n");

            await foreach (var response in agent.InvokeAsync(
                "What do you know about me?",
                agentThread))
            {
                response.Message.PrintChatMessageContent();
            }

            // Third conversation - preferences influence responses
            Console.WriteLine("\n[Conversation 3] Preferences influence responses:\n");
            await foreach (var response in agent.InvokeAsync(
                "Explain what machine learning is.",
                agentThread))
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
    /// Demonstrates user-scoped memories that persist across multiple threads
    /// </summary>
    private async Task Example2_UserScopedMemoriesAsync()
    {
        Utils.PrintSectionHeader("Example 2: User-Scoped Memories Across Threads");

        Console.WriteLine("User-scoped memories persist across multiple conversation threads.\n");

        var httpClient = httpClientFactory.CreateClient("mem0");

        // Create a Mem0 provider scoped to a specific user
        var mem0Provider = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U2",
            ApplicationId = "ShoppingAssistant"
        });

        // Clear previous memories for this user
        await mem0Provider.ClearStoredMemoriesAsync();

        ChatCompletionAgent agent = new()
        {
            Name = "ShoppingAssistant",
            Instructions = "You are a shopping assistant. Remember user preferences and provide personalized recommendations.",
            Kernel = kernel
        };

        // First thread - establish preferences
        Console.WriteLine("[Thread 1] Establishing preferences:\n");
        ChatHistoryAgentThread thread1 = new();
        thread1.AIContextProviders.Add(mem0Provider);

        try
        {
            await foreach (var response in agent.InvokeAsync(
                "I love outdoor activities and I'm looking for hiking gear. I prefer eco-friendly products.",
                thread1))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await thread1.DeleteAsync();
        }

        // Second thread - memories carry over
        Console.WriteLine("\n[Thread 2] New thread, same memories:\n");
        ChatHistoryAgentThread thread2 = new();
        thread2.AIContextProviders.Add(mem0Provider);

        try
        {
            await foreach (var response in agent.InvokeAsync(
                "I need a new backpack. What would you recommend?",
                thread2))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await thread2.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));
    }

    /// <summary>
    /// Demonstrates thread-scoped memories that are isolated to a single conversation
    /// </summary>
    private async Task Example3_ThreadScopedMemoriesAsync()
    {
        Utils.PrintSectionHeader("Example 3: Thread-Scoped Memories");

        Console.WriteLine("Thread-scoped memories are only available within the specific thread.\n");

        var httpClient = httpClientFactory.CreateClient("mem0");

        ChatCompletionAgent agent = new()
        {
            Name = "ProjectAssistant",
            Instructions = "You are a project management assistant. Track project-specific details and provide relevant insights.",
            Kernel = kernel
        };

        // First thread - Project Alpha
        Console.WriteLine("[Thread: Project Alpha]\n");
        ChatHistoryAgentThread threadAlpha = new();

        // Create Mem0 provider scoped to this specific thread
        var mem0ProviderAlpha = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U3",
            ScopeToPerOperationThreadId = true // Use the agent thread's ID
        });

        threadAlpha.AIContextProviders.Add(mem0ProviderAlpha);

        try
        {
            await foreach (var response in agent.InvokeAsync(
                "This is Project Alpha. Budget: $50,000. Timeline: 6 months. Team size: 5 developers.",
                threadAlpha))
            {
                response.Message.PrintChatMessageContent();
            }

            Console.WriteLine("\n");
            await foreach (var response in agent.InvokeAsync(
                "What's the budget for this project?",
                threadAlpha))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await threadAlpha.DeleteAsync();
        }

        // Second thread - Project Beta (different memories)
        Console.WriteLine("\n[Thread: Project Beta]\n");
        ChatHistoryAgentThread threadBeta = new();

        var mem0ProviderBeta = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U3",
            ScopeToPerOperationThreadId = true // Different thread ID
        });

        threadBeta.AIContextProviders.Add(mem0ProviderBeta);

        try
        {
            await foreach (var response in agent.InvokeAsync(
                "This is Project Beta. Budget: $100,000. Timeline: 12 months. Team size: 10 developers.",
                threadBeta))
            {
                response.Message.PrintChatMessageContent();
            }

            Console.WriteLine("\n");
            await foreach (var response in agent.InvokeAsync(
                "What's the budget for this project?",
                threadBeta))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            await threadBeta.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));
    }

    /// <summary>
    /// Demonstrates customizing the context prompt for Mem0 memories
    /// </summary>
    private async Task DemonstrateCustomContextPrompt()
    {
        Utils.PrintSectionHeader("Custom Context Prompt");

        var httpClient = httpClientFactory.CreateClient("mem0");

        var mem0Provider = new Mem0Provider(httpClient, options: new Mem0ProviderOptions
        {
            UserId = "U4",
            ContextPrompt = "The following information has been learned about the user from previous conversations. Use this to provide highly personalized responses:"
        });

        ChatCompletionAgent agent = new()
        {
            Name = "CustomPromptAssistant",
            Instructions = "You are a personalized assistant.",
            Kernel = kernel
        };

        ChatHistoryAgentThread agentThread = new();
        agentThread.AIContextProviders.Add(mem0Provider);

        try
        {
            await foreach (var response in agent.InvokeAsync(
                "I'm a data scientist who loves Python and prefers visual explanations.",
                agentThread))
            {
                response.Message.PrintChatMessageContent();
            }

            Console.WriteLine("\n");
            await foreach (var response in agent.InvokeAsync(
                "Explain gradient descent to me.",
                agentThread))
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
}