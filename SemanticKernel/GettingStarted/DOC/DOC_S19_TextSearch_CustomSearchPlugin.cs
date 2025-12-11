using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


public class DOC_S19_TextSearch_CustomSearchPlugin(Kernel kernel, ITextSearch textSearch) : ITest
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
        // https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/reference/query-parameters
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "GetTextSearchResults",
            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true, DefaultValue = 5 },
                new KernelParameterMetadata("include_domain") { Description = "Domain", IsRequired = false, DefaultValue = 5 },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) }
        };

        // Tavily: topic,time_range,days,include_domain,exclude_domain
        // Google: cr,dateRestrict,exactTerms,excludeTerms,filter,gl,hl,linkSite,lr,orTerms,rights,siteSearch

        // Build a text search plugin and add to the kernel
        var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin",
            "Search specified site", [textSearch.CreateGetTextSearchResults(options: options)]);

        kernel.Plugins.Add(searchPlugin);

        var query = "What are the latest advancements in AI?";

        string promptTemplate = """
            {{#with (SearchPlugin-GetTextSearchResults query count include_domain)}}
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

        KernelArguments arguments = new() {
            { "query", query }, 
            { "count", 5 },
            { "include_domain", "visualstudiomagazine.com" }
        };

        HandlebarsPromptTemplateFactory promptTemplateFactory = new();

        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }
}