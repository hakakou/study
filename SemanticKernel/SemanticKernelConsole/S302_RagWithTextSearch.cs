using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class S302_RagWithTextSearch : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();

        ITextSearch textSearch = new GoogleTextSearch(
            searchEngineId: Conf.GoogleTextSearch.SearchEngineId,
            apiKey: Conf.GoogleTextSearch.ApiKey);

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