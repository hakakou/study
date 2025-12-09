using Azure;
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
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        Console.WriteLine("=== Semantic Kernel Agents Demo ===\n");

        await Example1_NonStreamingAgentInvocationAsync();
        await UseTemplateForChatCompletionAgent();
        //await Example2_StreamingAgentInvocationAsync();
    }

    private async Task Example1_NonStreamingAgentInvocationAsync()
    {
        Utils.PrintSectionHeader("Example 1: Non-Streaming Agent Invocation");

        // Define the agent
        ChatCompletionAgent agent =
            new()
            {
                Name = "Joker",
                Instructions = "You are good at telling jokes.",
                Kernel = kernel,
            };

        AgentThread? thread = null;

        thread = await agent.InvokeAgentAsync(thread, "Tell me a joke about a pirate.");
        await agent.InvokeAgentAsync(thread, "Now add some emojis to the joke.");
    }

    private async Task Example_InvokeAgentAsync()
    {
        Utils.PrintSectionHeader("With history");

        ChatCompletionAgent agent = new()
        {
            Name = "Joker",
            Instructions = "You are good at telling jokes.",
            Kernel = kernel,
        };

        var thread = new ChatHistoryAgentThread(
        [
           new ChatMessageContent(AuthorRole.User, "Tell me a joke about a pirate."),
           new ChatMessageContent(AuthorRole.Assistant, "Why did the pirate go to school? Because he wanted to improve his \"arrrrrrrrrticulation\""),
        ]);

        await agent.InvokeAgentAsync(thread, "Now add some emojis to the joke.");
        await agent.InvokeAgentAsync(thread, "Now make the joke sillier.");
    }

    public async Task UseTemplateForChatCompletionAgent()
    {
        // Define the agent
        PromptTemplateConfig templateConfig = KernelFunctionYaml.ToPromptTemplateConfig("""
            name: GenerateStory
            template: |
              Tell a story about {{$topic}} that is {{$length}} sentences long.
            template_format: semantic-kernel
            description: A function that generates a story about a topic.
            input_variables:
              - name: topic
                description: The topic of the story.
                is_required: true
              - name: length
                description: The number of sentences in the story.
                is_required: true
            output_variable:
              description: The generated story.
            execution_settings:
              default:
                temperature: 0.6
            """);
        KernelPromptTemplateFactory templateFactory = new();

        ChatCompletionAgent agent = new(templateConfig, templateFactory)
        {
            Kernel = kernel,
            Arguments = new()
                    {
                        { "topic", "Dog" },
                        { "length", "3" },
                    }
        };


        //ChatCompletionAgent agent =
        //    new(templateFactory: new KernelPromptTemplateFactory(),
        //        templateConfig: new("Tell a story about {{$topic}} that is {{$length}} sentences long.")
        //        { TemplateFormat = PromptTemplateConfig.SemanticKernelTemplateFormat })
        //    {
        //        Kernel = kernel,
        //        Name = "StoryTeller",
        //        Arguments = new KernelArguments()
        //            {
        //                { "topic", "Dog" },
        //                { "length", "3" },
        //            }
        //    };


        // Invoke the agent with the default arguments.
        await InvokeAgentAsync();

        // Invoke the agent with the override arguments.
        await InvokeAgentAsync(
            new()
            {
                { "topic", "Cat" },
                { "length", "3" },
            });

        // Local function to invoke agent and display the conversation messages.
        async Task InvokeAgentAsync(KernelArguments? arguments = null)
        {
            // Invoke the agent without any messages, since the agent has all that it needs via the template and arguments.
            await foreach (ChatMessageContent content in agent.InvokeAsync(options: new() { KernelArguments = arguments }))
            {
                content.PrintChatMessageContent();
            }
        }
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
