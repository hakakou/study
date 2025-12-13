using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;

public class DOC_S17_BasicSearchPlugin(Kernel kernel, ITextSearch textSearch) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Trace));
        services.AddKernel()
            .DefaultChatCompletion()
            .GoogleTextSearch();
    }

    public async Task Run()
    {
        // Creates a plugin from an ITextSearch implementation.
        // The plugin will have a single function called `Search`
        // which will return a IEnumerable{String}
        var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");

        kernel.Plugins.Add(searchPlugin);

        var query = "What is Ideas Forward, Greece?";
        var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";
        KernelArguments arguments = new() { { "query", query } };

        Console.WriteLine(await kernel.InvokePromptAsync(prompt, arguments));
    }
}