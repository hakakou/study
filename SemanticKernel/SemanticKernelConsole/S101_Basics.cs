using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0010

public class S101_Basics : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();

        KernelArguments arguments = new(
            new OpenAIPromptExecutionSettings
            {
                // MaxTokens = 100,
                ResponseFormat = "json_object"
            }) { { "topic", "chocolate" } };

        var res = await kernel.InvokePromptAsync(
            "Create a recipe for a {{$topic}} cake in JSON format", arguments);
        Console.WriteLine(res);

        /*
        var res = kernel.InvokePromptStreamingAsync(
            "Create a recipe for a {{$topic}} cake in JSON format", arguments);

        await foreach (StreamingKernelContent response in res)
            Console.WriteLine(response);
        */

        Console.WriteLine();
    }
}