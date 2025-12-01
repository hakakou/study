using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json.Serialization;

// https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp

public class DOC_S03_GettingStarted_Lights_Anthropic(
    Kernel kernel, IChatCompletionService chatCompletionService) : ITest
{
    public static void Build(IServiceCollection services)
    {
        // https://github.com/tghamm/Anthropic.SDK
        services.AddKernel().DefaultAnthropic()
            .Plugins.AddFromType<LightsPlugin>("Lights");
    }

    public async Task Run()
    {
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ModelId = Conf.AzureAnthropic.DeploymentName,
        };

        // Create a history store the conversation
        var history = new ChatHistory();

        // Initiate a back-and-forth chat
        string? userInput;
        do
        {
            // Collect user input
            Console.Write("User > ");
            userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput))
                break;

            // Add user input
            history.AddUserMessage(userInput);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel); // Required for auto function calling

            // Print the results
            Console.WriteLine("Assistant > " + result);
            history.Add(result);
        } while (true);

        history.PrintChatHistory();
    }

    public class LightsPlugin
    {
        // Mock data for the lights
        private readonly List<LightModel> lights = new()
   {
      new LightModel { Id = 1, Name = "Table Lamp", IsOn = false },
      new LightModel { Id = 2, Name = "Porch light", IsOn = false },
      new LightModel { Id = 3, Name = "Chandelier", IsOn = true }
   };

        [KernelFunction("get_lights")]
        [Description("Gets a list of lights and their current state")]
        [return: Description("An array of lights")]
        public async Task<List<LightModel>> GetLightsAsync()
        {
            return lights;
        }

        [KernelFunction("change_state")]
        [Description("Changes the state of the light")]
        [return: Description("The updated state of the light; will return null if the light does not exist")]
        public async Task<LightModel?> ChangeStateAsync(int id, bool isOn)
        {
            var light = lights.FirstOrDefault(light => light.Id == id);

            if (light == null)
            {
                return null;
            }

            // Update the light with the new state
            light.IsOn = isOn;

            return light;
        }

        [KernelFunction("create_light")]
        [Description("Creates a new light")]
        [return: Description("The new light and it's state")]
        public async Task<LightModel?> CreateLightAsync(string lightName, bool isOn)
        {
            var light = new LightModel
            {
                Id = lights.Count + 1,
                Name = lightName,
                IsOn = isOn
            };
            lights.Add(light);

            return light;
        }
    }

    public class LightModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_on")]
        public bool? IsOn { get; set; }
    }
}