using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class DOC_S19_TextSearch : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        ITextSearch textSearch = new BingTextSearch("303882ec48a04a2089d34a2ed6441f3c");

        // https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/reference/query-parameters
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "GetSiteResults",
            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true, DefaultValue = 5 },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) }
        };

        var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin",
            "Search specified site", [textSearch.CreateGetTextSearchResults(options)]);
        kernel.Plugins.Add(searchPlugin);

        var query = "What is Ideas Forward, Greece?";
        //var query = "What is the Semantic Kernel?";
        string promptTemplate = """
            {{#with (SearchPlugin-GetSiteResults query count)}}
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
        // Only include results from techcommunity.microsoft.com.

        // Why are only 2 results returned always?

        KernelArguments arguments = new() { { "query", query }, { "count", 5 } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();
        Console.WriteLine(await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        ));
    }
}