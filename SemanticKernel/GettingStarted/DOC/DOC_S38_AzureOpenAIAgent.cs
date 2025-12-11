using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Agents.AzureAI;

public class DOC_S38_AzureOpenAIAgent : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel().DefaultChatCompletion();
    }

    public async Task Run()
    {
        // 1. Create a PersistentAgentsClient
        PersistentAgentsClient agentsClient = AzureAIAgent.CreateAgentsClient(
            Conf.MicrosoftFoundry.Endpoint, new AzureCliCredential());

        // 2. Upload a local file so Code Interpreter can use it
        //    (adjust path and purpose as needed)
        string localFilePath = "media/housing.csv";

        /*
        // Upload the file for use by agents
        PersistentAgentFileInfo uploadedFile = await agentsClient.Files.UploadFileAsync(
                localFilePath, PersistentAgentFilePurpose.Agents);
        var fileId = uploadedFile.Id;

        // 2. Create the persistent agent definition in Azure
        */

        var fileId = "assistant-AAQVrDAyrYTGPCi1nzBTFi";
        PersistentAgent definition = await agentsClient.Administration.CreateAgentAsync(
            model: "gpt-4.1", // Note that gpt-5.1-chat does seem to support code interpreter
            name: "Demo Agent",
            description: "Example agent that can run code interpreter.",
            instructions: "You are a data analysis assistant.Provide clear explanations of your analysis.",
            tools: [new CodeInterpreterToolDefinition()],
            toolResources: new()
            {
                CodeInterpreter = new()
                {
                    FileIds = { fileId },
                }
            }
         );

        //PersistentAgent definition = 
        //    await agentsClient.Administration.GetAgentAsync("asst_ZZToNQVLZq9dg3HAKCZwMj2c");



        // 4. Wrap the persistent agent in a Semantic Kernel AzureAIAgent
        AzureAIAgent agent = new(definition, agentsClient);

        // 5. Create a thread for this conversation
        AzureAIAgentThread agentThread = new(agentsClient);
        await agent.InvokeAgentAsync(agentThread, "examine for any trends.");
        await agentThread.DeleteAsync();
        await agent.Client.Administration.DeleteAgentAsync(agent.Id);
    }
}
