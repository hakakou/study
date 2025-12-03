using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


[RunDirectly]
public class DOC_S18_TextSearch(Kernel kernel, ITextSearch textSearch) : ITest
{

    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion()
            .GoogleTextSearch();

        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
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
        var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        var promptTemplate = $$$"""
        {{#with (SearchPlugin-GetTextSearchResults query)}}  
            {{#each this}}  
            Name: {{Name}}
            Value: {{Value}}
            Link: {{Link}}
            -----------------
            {{/each}}  
        {{/with}}  

        {{query}}

        Write 5-7 points summarizing the topic above. Don't end with a conclusion.
        Include citations to the relevant information where it is referenced in the response.
        """;

        KernelArguments arguments = new() { { "query", "What is a wiki?" } };

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