using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI.Files;
using OpenAI.VectorStores;
using System.ClientModel;

public class S211_OpenAIFileSearch : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var provider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(Program.ApiKey));

        var agent = await OpenAIAssistantAgent.CreateAsync(
            provider,
            definition: new OpenAIAssistantDefinition("gpt-4o-mini")
            {
                EnableFileSearch = true,
                Instructions = "Answer questions only about the included file.",
            },
            kernel: builder.Build());

        // Upload File
        OpenAIFileClient fileClient = provider.Client.GetOpenAIFileClient();
        await using Stream stream = File.OpenRead(@"c:\Users\harry\OneDrive - TELEMATIC MEDICAL APPLICATIONS\ePokratis\iso14971.pdf");
        OpenAIFile fileInfo = await fileClient.UploadFileAsync(stream, "iso14971.pdf", FileUploadPurpose.Assistants);

        // Create a vector-store
        VectorStoreClient vectorStoreClient = provider.Client.GetVectorStoreClient();
        CreateVectorStoreOperation vectorResult =
            await vectorStoreClient.CreateVectorStoreAsync(waitUntilCompleted: false,
                new VectorStoreCreationOptions()
                {
                    FileIds = { fileInfo.Id },
                    // Metadata = { { AssistantSampleMetadataKey, bool.TrueString } }
                });

        string threadId = await agent.CreateThreadAsync(
            new OpenAIThreadCreationOptions
            {
                VectorStoreId = vectorResult.VectorStoreId,
            });

        await agent.InvokeMessage(threadId, "Who are the best customers?");
        await agent.InvokeMessage(threadId, "What is a residual risk evaluation?");

        await agent.DeleteThreadAsync(threadId);
        await agent.DeleteAsync();
        await vectorStoreClient.DeleteVectorStoreAsync(vectorResult.VectorStoreId);
        await fileClient.DeleteFileAsync(fileInfo.Id);
    }
}