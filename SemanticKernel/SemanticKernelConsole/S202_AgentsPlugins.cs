using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SharedConfig;
using System;
using System.Text.Json.Serialization;
#pragma warning disable SKEXP0110

public class S202_AgentsPlugins : ITest
{
    public async Task Run()
    {
        var agent = new ChatCompletionAgent()
        {
            Name = "Seller",
            Instructions = "You are a seller. Answer questions about products.",
            Kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: "gpt-4o",
                    apiKey: Conf.OpenAI.ApiKey)
                .Build(),
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
        };

        agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<ProductPlugin>());

        ChatHistory chat = [];
        ChatMessageContent message = new(AuthorRole.User, "what products do you have and their price?");
        chat.Add(message);

        message.ConsoleOutputAgentChatMessage();
        await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
        {
            chat.Add(response);
            response.ConsoleOutputAgentChatMessage();

            var inp = Console.ReadLine();
            chat.Add(new(AuthorRole.User, inp));
        }
    }

    public sealed class ProductPlugin
    {
        [KernelFunction]
        public List<ProductModel> GetProducts()
        {
            return new List<ProductModel>() {
                new ProductModel { Id = 1, Name = "A" },
                new ProductModel { Id = 2, Name = "B" },
                new ProductModel { Id = 3, Name = "C" },
                new ProductModel { Id = 4, Name = "D" },
            };
        }

        [KernelFunction]
        public bool IsAvailable(int id)
        {
            return id % 2 == 0;
        }

        [KernelFunction]
        public decimal GetPrice(int id)
        {
            return id * 10;
        }
    }

    public class ProductModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}