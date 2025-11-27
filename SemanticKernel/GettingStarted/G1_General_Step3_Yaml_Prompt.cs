using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using SharedConfig;
using Spectre.Console;
using static SharedConfig.Conf;


public class G1_General_Step3_Yaml_Prompt : ITest
{
    public async Task Run()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatClient(apiKey: Conf.OpenAI.ApiKey, modelId: "gpt-5-nano-2025-08-07")
            .Build();

        {
            var templateConfig = new PromptTemplateConfig()
            {
                Name = "Product",
                Description = "Product name generator",
                Template = @"What is a good name for a company that makes {{$product}}?",
                TemplateFormat = "semantic-kernel",
                InputVariables = [
                    new() { Name = "product", Description = "The product", IsRequired = true }
                ]
            };

            var function = kernel.CreateFunctionFromPrompt(templateConfig);
            Console.WriteLine(await kernel.InvokeAsync(function, arguments: new()
            {
                { "product", "widgets" },
            }));
        }

        {
            // Load prompt from resource
            var yaml = """
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

            // var templateConfig = KernelFunctionYaml.ToPromptTemplateConfig(yaml);
            KernelFunction function = kernel.CreateFunctionFromPromptYaml(yaml);

            // Invoke the prompt function and display the result
            Console.WriteLine(await kernel.InvokeAsync(function, arguments: new()
            {
                { "topic", "Dog" },
                { "length", "5" },
            }));
        }
    }
}