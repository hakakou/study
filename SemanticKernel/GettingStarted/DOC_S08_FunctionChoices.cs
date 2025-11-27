using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System.ComponentModel;
using System.Text.Json.Serialization;


namespace DOC_S08_FunctionChoices;

public class DOC_S08_FunctionChoices : ITest
{
    public async Task Run()
    {
        await DemonstrateAutoFunctionChoiceBehavior();
        await DemonstrateRequiredFunctionChoiceBehavior();
        await DemonstrateNoneFunctionChoiceBehavior();
        await DemonstrateAutoWithSpecificFunctions();
        await DemonstrateYamlPromptWithFunctionChoice();
    }

    private async Task DemonstrateAutoFunctionChoiceBehavior()
    {
        Utils.PrintSectionHeader("Auto Function Choice Behavior");
        
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        kernel.Plugins.AddFromType<TimePlugin>();

        var autoSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: null),
        };

        var autoHistory = new ChatHistory();
        autoHistory.AddUserMessage("Based on the current time, are the lights correctly on or off?");

        var autoResult = await chatCompletionService.GetChatMessageContentAsync(
            autoHistory,
            executionSettings: autoSettings,
            kernel: kernel);

        Console.WriteLine(autoResult.Content);
    }

    private async Task DemonstrateRequiredFunctionChoiceBehavior()
    {
        Utils.PrintSectionHeader("Required Function Choice Behavior");
        
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        kernel.Plugins.AddFromType<TimePlugin>();

        var requiredSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
        };

        var requiredHistory = new ChatHistory();
        requiredHistory.AddUserMessage("What are the type of lights that exist?");

        var requiredResult = await chatCompletionService.GetChatMessageContentAsync(
            requiredHistory,
            executionSettings: requiredSettings,
            kernel: kernel);

        Console.WriteLine(requiredResult.Content);
    }

    private async Task DemonstrateNoneFunctionChoiceBehavior()
    {
        Utils.PrintSectionHeader("None Function Choice Behavior");
        
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        kernel.Plugins.AddFromType<TimePlugin>();

        var noneSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.None()
        };

        var noneHistory = new ChatHistory();
        noneHistory.AddUserMessage("Which functions would you use to adjust the lights based on time of day?");

        var noneResult = await chatCompletionService.GetChatMessageContentAsync(
            noneHistory,
            executionSettings: noneSettings,
            kernel: kernel);

        Console.WriteLine(noneResult.Content);
    }

    private async Task DemonstrateAutoWithSpecificFunctions()
    {
        Utils.PrintSectionHeader("Auto Function Choice Behavior with Specific Functions");
        
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<LightsPlugin>("Lights");
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        KernelFunction getLights = kernel.Plugins.GetFunction("Lights", "get_lights");
        KernelFunction getCurrentTime = kernel.Plugins.GetFunction("TimePlugin", "GetCurrentUtcTime");

        PromptExecutionSettings settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                functions: [getCurrentTime, getLights])
        };

        var result = await kernel.InvokePromptAsync("Given the current time of day, what lights are available and what are their states?", new(settings));

        Console.WriteLine(result);
    }

    private async Task DemonstrateYamlPromptWithFunctionChoice()
    {
        Utils.PrintSectionHeader("YAML Prompt with Function Choice Behavior");
        
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: Conf.OpenAI.ApiKey);
        builder.Plugins.AddFromType<LightsPlugin>("Lights");
        builder.Plugins.AddFromType<TimePlugin>();

        Kernel kernel = builder.Build();

        string promptTemplateConfig = """
            template_format: semantic-kernel
            template: Given the current time of day, what lights are currently available and should they be on or off?
            execution_settings:
              default:
                function_choice_behavior:
                  type: auto
            """;

        KernelFunction promptFunction = KernelFunctionYaml.FromPromptYaml(promptTemplateConfig);
        var result = await kernel.InvokeAsync(promptFunction);
        Console.WriteLine(result);
    }
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

public class TimePlugin
{
    /// <summary>
    /// Retrieves the current time in UTC.
    /// </summary>
    /// <returns>The current time in UTC. </returns>
    [KernelFunction, Description("Retrieves the current time in UTC.")]
    public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
}
