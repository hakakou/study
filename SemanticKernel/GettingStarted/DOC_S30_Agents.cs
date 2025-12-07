using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

public class DOC_S30_Agents(Kernel kernel) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion();

        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Information));
    }

    public async Task Run()
    {
        Console.WriteLine("=== Semantic Kernel Agents Demo ===\n");

        //await Example1_NonStreamingAgentInvocationAsync();
        await Example2_StreamingAgentInvocationAsync();
    }

    /// <summary>
    /// Example 1: Non-streaming agent invocation demonstrating different ways to pass messages
    /// </summary>
    private async Task Example1_NonStreamingAgentInvocationAsync()
    {
        Utils.PrintSectionHeader("Example 1: Non-Streaming Agent Invocation");

        // Create an agent with specific instructions
        var agent = new ChatCompletionAgent()
        {
            Name = "TravelAdvisor",
            Instructions = "You are a helpful travel advisor. Provide concise and accurate information about travel destinations. Never more that 4 sentances.",
            Kernel = kernel,
        };

        var result = await agent.InvokeAsync("What is the capital of France?").FirstAsync();
        var thread = result.Thread;
        result.Message.PrintChatMessageContent();

        // 2. Invoke with a string (converted to User message)
        Console.WriteLine("[Invocation 2] Agent with string input:\n");
        await foreach (var response in agent.InvokeAsync("What is the capital of France?", thread))
        {
            Console.WriteLine($"{response.Thread.Id}");
            response.Message.PrintChatMessageContent();
        }
        Console.WriteLine();

        await thread.DeleteAsync();

        return;

        // 3. Invoke with a ChatMessageContent object
        Console.WriteLine("[Invocation 3] Agent with ChatMessageContent:\n");
        var message = new ChatMessageContent(AuthorRole.User, "What are the must-see attractions in Paris?");
        await foreach (var response in agent.InvokeAsync(message))
        {
            response.Message.PrintChatMessageContent();
        }
        Console.WriteLine();

        // 4. Invoke with multiple ChatMessageContent objects
        Console.WriteLine("[Invocation 4] Agent with multiple messages:\n");
        var messages = new List<ChatMessageContent>()
        {
            new(AuthorRole.System, "Refuse to answer questions about accommodations in France."),
            new(AuthorRole.User, "Where should I stay in Paris?")
        };
        await foreach (var response in agent.InvokeAsync(messages))
        {
            response.Message.PrintChatMessageContent();
        }
        Console.WriteLine(new string('-', 80));
    }

    /// <summary>
    /// Example 2: Streaming agent invocation for real-time response processing
    /// </summary>
    private async Task Example2_StreamingAgentInvocationAsync()
    {
        Utils.PrintSectionHeader("Example 2: Streaming Agent Invocation");

        // Create an agent specialized in providing programming advice
        var agent = new ChatCompletionAgent()
        {
            Name = "CodeMentor",
            Instructions = "You are an experienced programming mentor. Provide clear, concise coding guidance and best practices.",
            Kernel = kernel,
        };

        // 1. Stream without any parameters
        Console.WriteLine("[Streaming 1] Agent with no input message:\n");
        await foreach (var chunk in agent.InvokeStreamingAsync())
        {
            var threadId = chunk.Thread.Id;
            Console.Write(chunk.Message.Content);
        }
        Console.WriteLine("\n");

        // 2. Stream with a string input
        Console.WriteLine("[Streaming 2] Agent with string input:\n");
        await foreach (StreamingChatMessageContent chunk in agent.InvokeStreamingAsync("What is dependency injection?"))
        {
            Console.Write(chunk.Content);
        }
        Console.WriteLine("\n");

        // 3. Stream with a ChatMessageContent object
        Console.WriteLine("[Streaming 3] Agent with ChatMessageContent:\n");
        var message = new ChatMessageContent(AuthorRole.User, "Explain SOLID principles briefly.");
        await foreach (StreamingChatMessageContent chunk in agent.InvokeStreamingAsync(message))
        {
            Console.Write(chunk.Content);
        }
        Console.WriteLine("\n");

        // 4. Stream with multiple messages (conversation context)
        Console.WriteLine("[Streaming 4] Agent with conversation context:\n");
        var conversation = new List<ChatMessageContent>()
        {
            new(AuthorRole.System, "Refuse to answer questions about JavaScript frameworks."),
            new(AuthorRole.User, "What's the best JavaScript framework for beginners?")
        };
        await foreach (StreamingChatMessageContent chunk in agent.InvokeStreamingAsync(conversation))
        {
            Console.Write(chunk.Content);
        }
        Console.WriteLine("\n" + new string('-', 80));
    }
}
