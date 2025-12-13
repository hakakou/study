using Json.More;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.Transforms;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

[RunDirectly]
public class DOC_S41_ConcurrentWithStructuredOutput(Kernel kernel, IChatCompletionService chatCompletionService) : ITest
{

    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        // Define the agents
        ChatCompletionAgent agent1 = new()
        {
            Name = "ProductDesigner",
            Instructions = "You are a product designer. Given an idea, propose a concrete product with specific features, target audience, and a catchy name.",
            Description = "An expert in turning ideas into concrete product proposals",
            Kernel = kernel,
        };

        ChatCompletionAgent agent2 = new()
        {
            Name = "BusinessFeasibilityAnalyst",
            Instructions = """
            You are a business feasibility analyst. Given an idea, evaluate whether this 
            product would work in the real market. Consider production costs, 
            competition, and demand and possible revenue per year. Make a list of competitors or simular products if existing.
            """,
            
            Description = "An expert in evaluating product feasibility and market viability",
            Kernel = kernel
        };

        ChatCompletionAgent agent3 = new()
        {
            Name = "ViralMarketingStrategist",
            Instructions = "You are a viral marketing strategist. Given an idea, assess whether this product could self-market in a viral way. Consider shareability, social media appeal, and word-of-mouth potential.",
            Description = "An expert in viral marketing and organic growth potential",
            Kernel = kernel,
        };

        // Define the orchestration with transform
        StructuredOutputTransform<Analysis> outputTransform = new(chatCompletionService,
                new OpenAIPromptExecutionSettings { ResponseFormat = typeof(Analysis) });

        ConcurrentOrchestration<string, Analysis> orchestration =
            new(agent1, agent2, agent3)
            {
                ResultTransform = outputTransform.TransformAsync,
            };

        // Start the runtime
        InProcessRuntime runtime = new();
        await runtime.StartAsync();

        // Run the orchestration

        OrchestrationResult<Analysis> result = await orchestration.InvokeAsync("""
            "A mobile app that make a summary of your day based on pictures you take throughout the day.           
            """, runtime);
            //AI does some type of recognition on the pics taken.

        Analysis output = await result.GetValueAsync();
        Console.WriteLine(output.AsJson());

        await runtime.RunUntilIdleAsync();
    }


    private sealed class Analysis
    {
        public string ProductProposals { get; set; }
        public string FeasibilityAssessments { get; set; }
        public string ViralPotentialAnalysis { get; set; }
        public string EstimatedRevenuePerYear { get; set; }
        public string CompetitorsOrSimularProducts { get; set; }
    }





}