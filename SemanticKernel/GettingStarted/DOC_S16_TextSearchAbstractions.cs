using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;


#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


public class DOC_S16_TextSearchAbstractions : ITest
{
    public async Task Run()
    {
        Utils.PrintSectionHeader("Google Text Search");

        ITextSearch textSearch = new GoogleTextSearch(
            searchEngineId: Conf.GoogleTextSearch.SearchEngineId,
            apiKey: Conf.GoogleTextSearch.ApiKey);

        var query = "What is the Semantic Kernel?";

        // Search and return results
        KernelSearchResults<string> searchResults =
            await textSearch.SearchAsync(query, new() { Top = 4 });

        await foreach (string result in searchResults.Results)
        {
            Console.WriteLine(result);
        }

        // Search and return results as TextSearchResult items
        KernelSearchResults<TextSearchResult> textResults =
            await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4 });

        Utils.PrintSectionHeader("Google Text Search Results as TextSearchResult");

        await foreach (TextSearchResult result in textResults.Results)
        {
            Console.WriteLine($"Name:  {result.Name}");
            Console.WriteLine($"Value: {result.Value}");
            Console.WriteLine($"Link:  {result.Link}");
        }

        Utils.PrintSectionHeader("Google Text Search Results as Object");

        KernelSearchResults<object> objectResults =
            await textSearch.GetSearchResultsAsync("What is an elevator", new() { Top = 4 });

        await foreach (Google.Apis.CustomSearchAPI.v1.Data.Result result in objectResults.Results)
        {
            Console.WriteLine($"Title:       {result.Title}");
            Console.WriteLine($"Snippet:     {result.Snippet}");
            Console.WriteLine($"Link:        {result.Link}");
            Console.WriteLine($"DisplayLink: {result.DisplayLink}");
            Console.WriteLine($"Kind:        {result.Kind}");
        }
    }
}