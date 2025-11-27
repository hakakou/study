using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using SharedConfig;
using System.ClientModel;

public class S209_OpenAIAssistantAgentImages : ITest
{
    public async Task Run()
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-4o",
                apiKey: Conf.OpenAI.ApiKey)
            .Build();

        var clientProvider = OpenAIClientProvider.ForOpenAI(new ApiKeyCredential(Conf.OpenAI.ApiKey));

        //var agent = await OpenAIAssistantAgent.CreateAsync(
        //    clientProvider,
        //    definition: new OpenAIAssistantDefinition("gpt-4o-mini")
        //    {
        //        Instructions = "Answer questions about the menu.",
        //        Name = "Menu Assistant",
        //    },
        //    kernel: kernel);

        //agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());

        //string threadId = await agent.CreateThreadAsync(
        //    new OpenAIThreadCreationOptions { });

        //// Or upload an image:
        ////string fileId = await agent.UploadFileAsync(imageStream, "cat.jpg");
        ////new FileReferenceContent(fileId);

        //var message = new ChatMessageContent(AuthorRole.User,
        //    [new TextContent("Describe this image."),
        //     new ImageContent(new Uri("https://upload.wikimedia.org/wikipedia/commons/5/56/White_shark.jpg"))
        //    ]);

        //await agent.AddChatMessageAsync(threadId, message);
        //message.ConsoleOutputAgentChatMessage();

        //var r = agent.InvokeAsync(threadId);
        //await foreach (ChatMessageContent response in r)
        //{
        //    response.ConsoleOutputAgentChatMessage();
        //}
    }
}