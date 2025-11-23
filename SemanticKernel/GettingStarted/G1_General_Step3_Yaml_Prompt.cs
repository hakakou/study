using Microsoft.SemanticKernel;
using SharedConfig;

public class G1_General_Step3_Yaml_Prompt : ITest
{
    public async Task Run()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatClient(apiKey: Conf.OpenAI.ApiKey, modelId: "gpt-5-nano-2025-08-07")
            .Build();

        // Load prompt from resource
        var generateStoryYaml = """
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
                temperature: 1
                max_tokens: 2000
            """;
        KernelFunction function = kernel.CreateFunctionFromPromptYaml(generateStoryYaml);

        // Invoke the prompt function and display the result
        Console.WriteLine(await kernel.InvokeAsync(function, arguments: new()
        {
            { "topic", "Dog" },
            { "length", "5" },
        }));

        //// Load prompt from resource
        //var generateStoryHandlebarsYaml = EmbeddedResource.Read("GenerateStoryHandlebars.yaml");
        //function = kernel.CreateFunctionFromPromptYaml(generateStoryHandlebarsYaml, new HandlebarsPromptTemplateFactory());

        //// Invoke the prompt function and display the result
        //Console.WriteLine(await kernel.InvokeAsync(function, arguments: new()
        //{
        //    { "topic", "Cat" },
        //    { "length", "3" },
        //}));
    }
}