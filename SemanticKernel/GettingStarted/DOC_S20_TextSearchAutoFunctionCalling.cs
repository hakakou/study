using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;

#pragma warning disable SKEXP0050

public class DOC_S20_TextSearchAutoFunctionCalling(Kernel kernel, ITextSearch textSearch) : ITest
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
        var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin",
            "Search specified site", [CreateSearchBySite((TavilyTextSearch)textSearch)]);
        kernel.Plugins.Add(searchPlugin);

        // Invoke prompt and use text search plugin to provide grounding information
        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        KernelArguments arguments = new(settings);
        Console.WriteLine(await kernel.InvokePromptAsync(
            "format a ready-to- tsend newsletter layout with the subject: 'What are latest developments in AI last month?' Search and add links also", arguments));
    }

    private static KernelFunction CreateSearchBySite(TavilyTextSearch textSearch)
    {
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "Search",
            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true },
                new KernelParameterMetadata("include_domain") { Description = "Domain", IsRequired = false },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
        };

        // CreateSearch: Only strings
        // CreateGetTextSearchResults: JSON with name, link, value
        // CreateGetSearchResults: JSON with title, url, content, score (the native)
        return textSearch.CreateGetTextSearchResults(options, new TextSearchOptions() { Top = 10 });
    }
}