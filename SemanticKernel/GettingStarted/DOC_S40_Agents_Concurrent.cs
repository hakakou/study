using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

[RunDirectly]
public class DOC_S40_Agents_Concurrent(Kernel kernel, ILoggerFactory loggerFactory) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
        //services.AddLogging(c =>
        //    c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        // Description seems to be required!
        var coderCSharp = new ChatCompletionAgent()
        {
            Name = "CSharpCoder",
            Description = "An agent that writes C# code",
            Instructions = "Write a simple C# function based on the user message",
            Kernel = kernel,
        };

        var coderRust = new ChatCompletionAgent()
        {
            Name = "RustCoder",
            Description = "An agent that writes Rust code",
            Instructions = "Write a simple Rust function based on the user message",
            Kernel = kernel,
        };

        var coderDelphi = new ChatCompletionAgent()
        {
            Name = "DelphiCoder",
            Description = "An agent that writes Delphi code",
            Instructions = "Write a simple Delphi function based on the user message",
            Kernel = kernel,
        };

        var panels = new AgentPanels();

        // Define the orchestration
        ConcurrentOrchestration orchestration = new(coderRust, coderCSharp, coderDelphi)
        {
            LoggerFactory = loggerFactory,
            // Set one or the other callback to see results
            //ResponseCallback = Utils.PrintResponseCallback,
            StreamingResponseCallback = panels.StreamingResultCallback,
        };

        // Start the runtime
        InProcessRuntime runtime = new InProcessRuntime();

        await runtime.StartAsync();

        // Start panels in the background
        var panelsTask = panels.Start();

        OrchestrationResult<string[]> result =
            await orchestration.InvokeAsync("Write a hello world app", runtime);

        // Wait for the final result to ensure all streaming is complete
        string[] output = await result.GetValueAsync(TimeSpan.FromSeconds(30));

        // Optional: ensure runtime is fully idle
        await runtime.RunUntilIdleAsync();

        // Now that all agents have completed, wait for panels to finish rendering
        await panelsTask;
    }
}




