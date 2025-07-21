using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class S303_RagWithBingTextSearchIncludingTimeStampedCitationsAsync : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();

        ITextSearch textSearch = new GoogleTextSearch(
            searchEngineId: Conf.GoogleTextSearch.SearchEngineId,
            apiKey: Conf.GoogleTextSearch.ApiKey);

        var query = "Most important news today in Greece?";

        /*
        // Debug: Print raw search results
        KernelSearchResults<TextSearchResult> textResults =
            await textSearch.GetTextSearchResultsAsync(query);
        await foreach (TextSearchResult result in textResults.Results)
        {
            Console.WriteLine($"Name: {result.Name}");
            Console.WriteLine($"Value: {result.Value}");
            Console.WriteLine($"Link: {result.Link}");
            //Console.WriteLine($"Date Last Crawled: {result.}");
            Console.WriteLine("-----------------");
        }
        */

        // Creates a plugin from an ITextSearch implementation.
        // The plugin will have a single function called `GetSearchResults`
        // which will return a IEnumerable{TextSearchResult}
        var searchPlugin = textSearch.CreateWithGetSearchResults("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        var promptTemplate = $$$"""
        {{#with (SearchPlugin-GetSearchResults query)}}
            {{#each this}}
            {{this}}
            -----------------
            {{/each}}
        {{/with}}

        {{query}}

        Include citations to and the date of the relevant information where it is referenced in the response.
        Note: Local time is {{{DateTime.Now.ToString("u")}}}
        """;

        KernelArguments arguments = new() { { "query", query } };
        // package Microsoft.SemanticKernel.PromptTemplates.Handlebars
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();

        var res = await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        );

        Console.WriteLine(res);
    }
}