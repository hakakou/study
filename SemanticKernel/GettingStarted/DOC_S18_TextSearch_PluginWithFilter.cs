using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


public class DOC_S18_TextSearch_PluginWithFilter(Kernel kernel, ITextSearch textSearch) : ITest
{

    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion()
            .TavilyTextSearch();

        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        var filter = new TextSearchFilter().Equality("days", "30");

        var searchOptions = new TextSearchOptions() { Filter = filter };
        
        // Tavily: topic,time_range,days,include_domain,exclude_domain
        // Google: cr,dateRestrict,exactTerms,excludeTerms,filter,gl,hl,linkSite,lr,orTerms,rights,siteSearch

        // Build a text search plugin and add to the kernel
        var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin",
            "Search specified site", [textSearch.CreateGetTextSearchResults(searchOptions: searchOptions)]);

        kernel.Plugins.Add(searchPlugin);

        var promptTemplate = """
        {{#with (SearchPlugin-GetTextSearchResults query)}}  
            {{#each this}}  
            Name: {{Name}}
            Value: {{Value}}
            Link: {{Link}}
            -----------------
            {{/each}}  
        {{/with}}  

        {{query}}

        Include citations to the relevant information where it is referenced in the response.
        """;

        KernelArguments arguments = new() { { "query", "Latest gadgets?" } };

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