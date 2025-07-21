using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;


public class G1_General_Step1_CreateKernel : ITest
{
    public async Task Run()
    {
        // Create a kernel with OpenAI chat completion using ChatClient
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatClient(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        // Invoke the kernel with a chat prompt and display the result
        string chatPrompt = """
    <message role="user">What is Seattle?</message>
    <message role="system">Respond with JSON.</message>
    """;

        Console.WriteLine(await kernel.InvokePromptAsync(chatPrompt));

        // Example 1. Invoke the kernel with a prompt and display the result
        Console.WriteLine(await kernel.InvokePromptAsync("What color is the sky?"));
        Console.WriteLine();

        // Example 2. Invoke the kernel with a templated prompt and display the result
        KernelArguments arguments = new() { { "topic", "sea" } };
        Console.WriteLine(await kernel.InvokePromptAsync("What color is the {{$topic}}?", arguments));
        Console.WriteLine();

        // Example 3. Invoke the kernel with a templated prompt and stream the results to the display
        await foreach (var update in kernel.InvokePromptStreamingAsync(
            "What color is the {{$topic}}? Provide a detailed explanation.", arguments))
        {
            Console.Write(update);
        }

        Console.WriteLine(string.Empty);

        // Example 4. Invoke the kernel with a templated prompt and execution settings
        arguments = new(new OpenAIPromptExecutionSettings { MaxTokens = 500, Temperature = 0.5 })
            { { "topic", "dogs" } };
        Console.WriteLine(await kernel.InvokePromptAsync("Tell me a story about {{$topic}}", arguments));

        // Example 5. Invoke the kernel with a templated prompt and execution settings configured to return JSON
        arguments = new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object"
            }) { { "topic", "chocolate" } };
        
        Console.WriteLine(await kernel.InvokePromptAsync(
            "Create a recipe for a {{$topic}} cake in JSON format", arguments));
    }
}
