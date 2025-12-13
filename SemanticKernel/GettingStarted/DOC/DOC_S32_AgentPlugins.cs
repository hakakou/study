using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

public class DOC_S32_AgentPlugins(Kernel kernel) : ITest
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
        Utils.PrintSectionHeader("Agent Plugins Demo");

        Kernel agentKernel = kernel.Clone();

        // Import plug-in from type (stateless)
        agentKernel.ImportPluginFromType<TimePlugin>();

        // Import plug-in from object (stateful)
        agentKernel.ImportPluginFromObject(new WeatherPlugin("XXX"));

        // Create plug-in from a static function
        var functionFromMethod = agentKernel.CreateFunctionFromMethod(Greeting);
        var functionFromPrompt = agentKernel.CreateFunctionFromPrompt("What is the capital of {{$country}}?");

        // Add to the kernel
        agentKernel.ImportPluginFromFunctions("my_plugin", [functionFromMethod, functionFromPrompt]);

        // Create the agent with automatic function calling enabled
        var weatherAgent = new ChatCompletionAgent()
        {
            Name = "WeatherAssistant",
            Instructions = "You are a helpful weather and news assistant. Use the Greeting function before any answer.",
            Kernel = agentKernel,
            Arguments = new KernelArguments(
                new OpenAIPromptExecutionSettings()
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
        };

        // Create a thread for conversation
        AgentThread thread = null;

        try
        {
            // Example 1: Agent uses plugin for time information
            Console.WriteLine("\n[Example 1] Agent with Time Plugin:\n");
            await foreach (var response in weatherAgent.InvokeAsync("What time is it now?", thread))
            {
                thread = response.Thread;
                response.Message.PrintChatMessageContent();
            }

            // Example 2: Agent uses stateful plugin with credentials
            Console.WriteLine("\n[Example 2] Agent with Stateful Plugin:\n");
            await foreach (var response in weatherAgent.InvokeAsync("Get the weather forecast for Seattle", thread))
            {
                response.Message.PrintChatMessageContent();
            }

            // Example 3: Multiple plugin invocations in one query
            Console.WriteLine("\n[Example 3] Agent using multiple plugins:\n");
            await foreach (var response in weatherAgent.InvokeAsync("What's the local time and weather in Paris?", thread))
            {
                response.Message.PrintChatMessageContent();
            }
        }
        finally
        {
            // await thread.DeleteAsync();
        }

        Console.WriteLine("\n" + new string('-', 80));
    }

    public static string Greeting()
    {
        return "Hello from static method!";
    }

    /// <summary>
    /// Stateless plugin example - can be imported from type
    /// </summary>
    public class TimePlugin
    {
        [KernelFunction]
        [Description("Gets the current UTC time")]
        public string GetUTCCurrentTime()
        {
            return DateTime.UtcNow.ToString("u");
        }
    }

    /// <summary>
    /// Stateful plugin example - must be imported from object instance
    /// </summary>
    public class WeatherPlugin
    {
        private readonly string _credentials;

        public WeatherPlugin(string credentials)
        {
            _credentials = credentials;
        }

        [KernelFunction]
        [Description("Gets the weather forecast for a specified city")]
        public string GetWeatherForecast([Description("The city name")] string city)
        {
            // In a real scenario, this would use the credentials to call an actual weather API
            Console.WriteLine($"[WeatherPlugin] Using credentials: {_credentials?.Substring(0, Math.Min(10, _credentials.Length))}...");

            // Simulated weather data
            var random = new Random();
            var temp = random.Next(50, 85);
            var conditions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy" };
            var condition = conditions[random.Next(conditions.Length)];

            return $"Weather in {city}: {temp}°F, {condition}";
        }

        [KernelFunction]
        [Description("Gets the current temperature for a city")]
        public string GetCurrentTemperature([Description("The city name")] string city)
        {
            Console.WriteLine($"[WeatherPlugin] Authenticated with credentials: {_credentials?.Substring(0, Math.Min(10, _credentials.Length))}...");

            var random = new Random();
            var temp = random.Next(50, 85);
            return $"Current temperature in {city}: {temp}°F";
        }
    }
}