using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Chat;
using Spectre.Console;

public class S01_PromptTemplate : ITest
{
    public async Task Run()
    {
        var promptTemplate = new PromptTemplateConfig()
        {
            Name = "Product",
            Description = "Product name generator",
            Template = @"What is a good name for a company that makes {{product}}?",
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() { Name = "product", Description = "The product", IsRequired = true }
            ]
        };

        var product = "widgets";
        var prompt = promptTemplate.Template.Replace("{{product}}", product);
        AnsiConsole.WriteLine(prompt);

        // Package Microsoft.SemanticKernel.Yaml
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
    }
}
