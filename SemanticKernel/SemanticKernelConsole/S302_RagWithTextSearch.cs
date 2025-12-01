using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;


#pragma warning disable SKEXP0050

public class S302_RagWithTextSearch : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

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