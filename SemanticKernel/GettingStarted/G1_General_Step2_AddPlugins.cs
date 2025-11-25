using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System.ComponentModel;
using System.Text.Json.Serialization;


public class G1_General_Step2_AddPlugins : ITest
{
    public async Task Run()
    {
        // Azure AI Foundry: This platform extends the capabilities of Azure OpenAI by providing
        // access to a broader range of flagship models, including those from Cohere, Mistral AI,
        // Meta Llama, AI21 labs, and more. Azure AI Foundry allows customers to switch between
        // different models seamlessly without changing their code. It includes Azure OpenAI as part
        // of its offerings, enabling a unified service, endpoint, and credential management.

        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
            Conf.AzureAIFoundry.DeploymentName,
            Conf.AzureAIFoundry.Endpoint,
            Conf.AzureAIFoundry.ApiKey);

        // Azure OpenAI: This service provides access to advanced language models developed by
        // OpenAI, such as GPT-4, GPT-3, Codex, and others. It focuses on delivering language AI
        // capabilities with the security and enterprise features of Azure. Azure OpenAI ensures
        // compatibility with OpenAI's APIs and offers features like private networking, regional
        // availability, and responsible AI content filtering.
        //var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
        //    Conf.AzureOpenAI.DeploymentName,
        //    Conf.AzureOpenAI.Endpoint,
        //    Conf.AzureOpenAI.ApiKey);

        //var builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(
        //    modelId: "deepseek/deepseek-r1-0528-qwen3-8b",
        //    endpoint: new Uri("http://localhost:1234/v1"),
        //    apiKey: "");

        // builder.Services.AddLogging(services =>
        // services.AddConsole().SetMinimumLevel(LogLevel.Trace));


        builder.Plugins.AddFromType<TimeInformation>();
        //builder.Plugins.AddFromType<AppointmentPlugin>();
        var kernel = builder.Build();

        //KernelFunction function = kernel.CreateFunctionFromMethod(
        //    typeof(ComputerInformation).GetMethod("GetComputerName")!);
        //Console.WriteLine(await kernel.InvokeAsync(function));

        // Use kernel for templated prompts that invoke plugins directly
        Console.WriteLine(await kernel.InvokePromptAsync(
            "The current time is {{TimeInformation.GetCurrentTime}}. How many days until Easter?"));

        // Console.WriteLine(await kernel.InvokePromptAsync("How many days until Christmas?"));

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        string? userInput;
        var history = new ChatHistory();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        do
        {
            // Collect user input
            Console.Write("User > ");
            userInput = Console.ReadLine();

            // Add user input
            history.AddUserMessage(userInput);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);
        } while (userInput is not null);
    }

    public static class ComputerInformation
    {
        public static string GetComputerName() => Environment.MachineName;
    }

    public class TimeInformation
    {
        [KernelFunction]
        [Description("Retrieves the current time in Athens time.")]
        public string GetCurrentTime() => DateTime.Now.ToString("R");

    }

    public class Appointment
    {
        [KernelFunction]
        public AppointmentItem BookAppointment(
            [Description("Services provided")] AppointmentType[] services,
            string doctor,
            DateTime time,
            string name)
        {
            var app = new AppointmentItem
            {
                Id = new Random().Next(1000, 9999),
                Services = services,
                Doctor = doctor,
                Time = time,
                Name = name
            };
            Console.WriteLine($"Created appointment {app.Dump()}");
            return app;
        }

        [KernelFunction]
        public async Task<List<string>> GetDoctors()
        {
            return new List<string> { "Dr. Smith", "Dr. Johnson", "Dr. Brown" };
        }

        [KernelFunction]
        public async Task<List<DateTime>> GetTimes(string doctor)
        {
            if (doctor == "Dr. Smith")
            {
                return new List<DateTime>
                {
                    DateTime.Now.AddDays(1).Date.AddHours(9),
                    DateTime.Now.AddDays(1).Date.AddHours(10),
                    DateTime.Now.AddDays(1).Date.AddHours(11)
                };
            }
            else if (doctor == "Dr. Johnson")
            {
                return new List<DateTime>
                {
                    DateTime.Now.AddDays(1).Date.AddHours(13),
                    DateTime.Now.AddDays(1).Date.AddHours(14),
                    DateTime.Now.AddDays(1).Date.AddHours(15)
                };
            }
            else // Dr. Brown
            {
                return new List<DateTime>
                {
                    DateTime.Now.AddDays(2).Date.AddHours(16),
                    DateTime.Now.AddDays(2).Date.AddHours(17),
                    DateTime.Now.AddDays(2).Date.AddHours(18)
                };
            }
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AppointmentType
    {
        Checkup,
        Prescription,
        Urgent
    }

    public class AppointmentItem
    {
        public int Id { get; set; }
        public AppointmentType[] Services { get; set; }
        public string Doctor { get; set; }
        public DateTime Time { get; set; }
        public string Name { get; set; }
    }
}