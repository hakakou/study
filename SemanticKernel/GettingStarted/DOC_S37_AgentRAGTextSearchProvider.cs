using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0020

[RunDirectly]
public class DOC_S37_AgentRAGTextSearchProvider(
    Kernel kernel,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatClient()
            .DefaultEmbeddings();
    }

    public async Task Run()
    {
        Utils.PrintSectionHeader("Agent RAG: TextSearchProvider for Retrieval Augmented Generation");

        //Console.WriteLine("The TextSearchProvider allows agents to retrieve relevant documents");
        //Console.WriteLine("based on user input and inject them into the agent's context for more");
        //Console.WriteLine("informed responses. This example demonstrates all TextSearchProvider features:\n");
        //Console.WriteLine("- Automatic RAG (BeforeAIInvoke)");
        //Console.WriteLine("- On-Demand RAG (OnDemandFunctionCalling)");
        //Console.WriteLine("- Citations with source names and links");
        //Console.WriteLine("- Filtering by namespace");
        //Console.WriteLine("- Custom context formatting\n");

        // Step 1: Create vector store and TextSearchStore
        var vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator
        });

        // TextSearchStore is a ITextSearch
        var textSearchStore = new TextSearchStore<string>(
            vectorStore,
            collectionName: "FinancialData",
            vectorDimensions: 3072);

        await textSearchStore.UpsertDocumentsAsync(new[]
        {
            new TextSearchDocument
            {
                Text = "The financial results of Contoso Corp for 2023 is as follows:\n" +
                       "Income EUR 174,000,000\nExpenses EUR 152,000,000\nNet Profit EUR 22,000,000",
                SourceName = "Contoso 2023 Financial Report",
                SourceLink = "https://www.contoso.com/reports/2023.pdf",
                Namespaces = ["finance/annual"]
            },
            new TextSearchDocument
            {
                Text = "The financial results of Contoso Corp for 2024 is as follows:\n" +
                       "Income EUR 154,000,000\nExpenses EUR 142,000,000\nNet Profit EUR 12,000,000",
                SourceName = "Contoso 2024 Financial Report",
                SourceLink = "https://www.contoso.com/reports/2024.pdf",
                Namespaces = ["finance/annual"]
            },
            new TextSearchDocument
            {
                Text = "The Contoso Corporation is a multinational business with its headquarters in Paris. " +
                       "The company was founded in 1985 and employs over 50,000 people worldwide.",
                SourceName = "Contoso Company Profile",
                SourceLink = "https://www.contoso.com/about",
                Namespaces = ["corporate/info"]
            },
            new TextSearchDocument
            {
                Text = "Contoso Corp Q1 2024 results show strong growth: Revenue increased by 15% year-over-year. " +
                       "Operating margin improved to 18.5%.",
                SourceName = "Contoso Q1 2024 Earnings",
                SourceLink = "https://www.contoso.com/reports/2024-q1.pdf",
                Namespaces = ["finance/quarterly"]
            }
        });

        await DemonstrateAutomaticRAG(textSearchStore);
        await DemonstrateOnDemandRAG(textSearchStore);
        await DemonstrateNamespaceFiltering(vectorStore);
        await DemonstrateCustomFormatting(textSearchStore);
    }

    private async Task DemonstrateAutomaticRAG(TextSearchStore<string> textSearchStore)
    {
        // Configure TextSearchProvider with BeforeAIInvoke (default)
        var textSearchProvider = new TextSearchProvider(
            textSearchStore,
            options: new TextSearchProviderOptions
            {
                Top = 2,
                SearchTime = TextSearchProviderOptions.RagBehavior.BeforeAIInvoke,
                ContextPrompt = "Use the following information to answer the user's question:",
                IncludeCitationsPrompt = "Include citations with source names and links in your response."
            });

        var agent = new ChatCompletionAgent
        {
            Name = "FinancialAnalyst",
            Instructions = "You are a financial analyst. Answer questions based on the provided documents. " +
                          "Always cite your sources with names and links.",
            Kernel = kernel
        };

        var agentThread = new ChatHistoryAgentThread();
        agentThread.AIContextProviders.Add(textSearchProvider);
        await agent.InvokeAgentAsync(agentThread, "What were Contoso's financial results for 2024?");
        await agent.InvokeAgentAsync(agentThread, "Where is Contoso based and when was it founded?");
        await agentThread.DeleteAsync();
    }

    private async Task DemonstrateOnDemandRAG(TextSearchStore<string> textSearchStore)
    {
        var textSearchProvider = new TextSearchProvider(
            textSearchStore,
            options: new TextSearchProviderOptions
            {
                Top = 3,
                SearchTime = TextSearchProviderOptions.RagBehavior.OnDemandFunctionCalling,
                PluginFunctionName = "SearchFinancialDocuments",
                PluginFunctionDescription = "Search financial documents and company information to help answer questions about Contoso Corp.",
                ContextPrompt = "Relevant documents found:",
                IncludeCitationsPrompt = "Cite your sources with document names and links."
            });

        var agent = new ChatCompletionAgent
        {
            Name = "SmartAssistant",
            Instructions = "You are a helpful assistant. Use the search function when you need information about Contoso Corp. " +
                          "Always provide citations.",
            Kernel = kernel,
            UseImmutableKernel = true // Required for OnDemandFunctionCalling
        };

        var agentThread = new ChatHistoryAgentThread();
        agentThread.AIContextProviders.Add(textSearchProvider);

        await agent.InvokeAgentAsync(agentThread, "Compare Contoso's financial performance between 2023 and 2024");
        await agent.InvokeAgentAsync(agentThread, "How many employees does Contoso have?");
    }

    private async Task DemonstrateNamespaceFiltering(InMemoryVectorStore textSearchStore)
    {
        var filteredTextSearchStore = new TextSearchStore<string>(
            textSearchStore,
            collectionName: "FinancialData",
            vectorDimensions: 3072,
            options: new TextSearchStoreOptions
            {
                SearchNamespace = "finance/annual"
            });

       
        // Create a TextSearchStore with namespace filtering - only finance/annual documents
        var textSearchProvider = new TextSearchProvider(
            filteredTextSearchStore,
            options: new TextSearchProviderOptions
            {
                Top = 3,
                ContextPrompt = "Use only the annual financial reports below:",
            });

        var agent = new ChatCompletionAgent
        {
            Name = "AnnualReportAnalyst",
            Instructions = "You are an analyst focused on annual reports. Answer based only on annual financial data.",
            Kernel = kernel
        };

        var agentThread = new ChatHistoryAgentThread();
        agentThread.AIContextProviders.Add(textSearchProvider);

        try
        {
            Console.WriteLine("Namespace filter: 'finance/annual' (excludes quarterly reports and company info)\n");
            Console.WriteLine("Query: What financial data is available for Contoso?\n");
            await agent.InvokeAgentAsync(agentThread,
                "What financial data is available for Contoso? List all years you can find.");
        }
        finally
        {
            await agentThread.DeleteAsync();
        }
    }

    private async Task DemonstrateCustomFormatting(TextSearchStore<string> textSearchStore)
    {
        // Use custom context formatter
        var textSearchProvider = new TextSearchProvider(
            textSearchStore,
            options: new TextSearchProviderOptions
            {
                Top = 2,
                ContextFormatter = (searchResults) =>
                {
                    var formattedContext = new System.Text.StringBuilder();
                    formattedContext.AppendLine("═══════════════════════════════════════════════════════════════");
                    formattedContext.AppendLine("📚 RETRIEVED KNOWLEDGE BASE DOCUMENTS");
                    formattedContext.AppendLine("═══════════════════════════════════════════════════════════════");

                    int index = 1;
                    foreach (var result in searchResults)
                    {
                        formattedContext.AppendLine($"\n[Document {index}]");
                        formattedContext.AppendLine($"Source: {result.Name ?? "Unknown"}");
                        if (!string.IsNullOrEmpty(result.Link))
                        {
                            formattedContext.AppendLine($"Link: {result.Link}");
                        }
                        formattedContext.AppendLine($"Content:\n{result.Value}");
                        formattedContext.AppendLine(new string('-', 60));
                        index++;
                    }

                    formattedContext.AppendLine("\n📋 INSTRUCTIONS:");
                    formattedContext.AppendLine("• Base your answer strictly on the documents above");
                    formattedContext.AppendLine("• Reference documents by their number [Document N]");
                    formattedContext.AppendLine("• If information is not in the documents, say so clearly");

                    return formattedContext.ToString();
                }
            });

        var agent = new ChatCompletionAgent
        {
            Name = "DocumentAssistant",
            Instructions = "Answer based on the provided documents. Reference documents by their number.",
            Kernel = kernel
        };

        var agentThread = new ChatHistoryAgentThread();
        agentThread.AIContextProviders.Add(textSearchProvider);

        try
        {
            Console.WriteLine("Using custom formatter with structured document presentation\n");
            Console.WriteLine("Query: What is the trend in Contoso's profitability?\n");
            await agent.InvokeAgentAsync(agentThread, "What is the trend in Contoso's profitability?");
        }
        finally
        {
            await agentThread.DeleteAsync();
        }
    }
}
