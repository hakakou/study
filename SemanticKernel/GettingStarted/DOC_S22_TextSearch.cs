using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

#pragma warning disable SKEXP0050

public class DOC_S22_TextSearch(Kernel kernel, ITextSearch textSearch) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion()
            .BingTextSearch();

        services.AddLogging(c =>
            c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        // Build a text search plugin with Bing search and add to the kernel
        var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
        //var searchPlugin = KernelPluginFactory.CreateFromFunctions("SearchPlugin",
        //    "Search specified site", [CreateSearchBySite((BingTextSearch)textSearch)]);
        kernel.Plugins.Add(searchPlugin);

        // Invoke prompt and use text search plugin to provide grounding information
        OpenAIPromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };
        KernelArguments arguments = new(settings);
        Console.WriteLine(await kernel.InvokePromptAsync("What is the Semantic Kernel?", arguments));
    }

    private static KernelFunction CreateSearchBySite(BingTextSearch textSearch)
    {
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "Search",
            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
        };

        return textSearch.CreateSearch(options, new TextSearchOptions() { Top = 10 });
    }
}