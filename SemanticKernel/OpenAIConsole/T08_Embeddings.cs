using Azure.AI.OpenAI;

using Spectre.Console;
using System.ClientModel;

internal class T08_Embeddings
{
    public static async Task Run()
    {
        var azureClient = new AzureOpenAIClient(
                    new Uri(Conf.AzureFoundry.Endpoint),
                    new ApiKeyCredential(Conf.AzureFoundry.ApiKey));

        var embeddingClient = azureClient.GetEmbeddingClient("text-embedding-3-large");

        // Get embeddings for the sentences
        var sentence1 = await embeddingClient.GenerateEmbeddingAsync("She works in tech since 2010, after graduating");
        var sentence2 = await embeddingClient.GenerateEmbeddingAsync("Verify inputs don't exceed the maximum length");

        // Calculate the dot product of the embeddings
        double dot = 0.0;
        var floats1 = sentence1.Value.ToFloats().ToArray();
        var floats2 = sentence2.Value.ToFloats().ToArray();
        for (int n = 0; n < floats1.Length; n++)
        {
            dot += floats1[0] * floats2[0];
        }

        // 0,7546766102313995
        AnsiConsole.WriteLine($"Dot product of the embeddings: {dot}");
    }
}