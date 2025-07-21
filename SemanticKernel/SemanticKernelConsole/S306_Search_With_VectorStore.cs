using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

public class S306_Search_With_VectorStore : ITest
{
    public async Task Run(IKernelBuilder builder)
    {
        var kernel = builder.Build();
        var textEmbeddingGeneration = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        var inMemoryVectorStore = new InMemoryVectorStore();
        var collection = inMemoryVectorStore.GetCollection<Guid, DataModel>("records");
        await collection.CreateCollectionIfNotExistsAsync();

        // Create records and generate embeddings for them.

        foreach (var line in lines)
        {
            var embedding = await textEmbeddingGeneration.GenerateEmbeddingAsync(line);

            var guid = Guid.NewGuid();
            var record = new DataModel()
            {
                Key = guid,
                Text = line,
                Link = $"guid://{guid}",
                Tag = guid.ToByteArray()[0] % 2 == 0 ? "Even" : "Odd",
                Embedding = embedding
            };
            await collection.UpsertAsync(record);
        }

        // Create a text search instance using the InMemory vector store.
        var textSearch = new VectorStoreTextSearch<DataModel>(collection, textEmbeddingGeneration);

        // Search and return results as TextSearchResult items
        var query = "Ποιοι έχουν αγγλικά για ξένη γλώσσα και είναι τεκνο πολύτεκνης οικογένειας;";

        // 1st way: Get text search results
        //
        //KernelSearchResults<TextSearchResult> textResults =
        //    await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });
        //
        //await foreach (TextSearchResult result in textResults.Results)
        //{
        //    Console.WriteLine($"Name:  {result.Name}");
        //    Console.WriteLine($"Value: {result.Value}");
        //    Console.WriteLine($"Link:  {result.Link}");
        //}

        // 2nd way: Get vector search results

        //KernelSearchResults<object> searchResults =
        //    await textSearch.GetSearchResultsAsync(query, new() { Top = 1, Skip = 0, IncludeTotalCount = true });

        //await foreach (DataModel result in searchResults.Results)
        //    Console.WriteLine($"Name:  {result.Text}");

        // 3rd way: Get vector search results with filters
        var searchVector = await textEmbeddingGeneration.GenerateEmbeddingsAsync([query]);
        var searchResults = await collection.VectorizedSearchAsync(searchVector.First(), new() { Top = 3 });

        await foreach (VectorSearchResult<DataModel> r in searchResults.Results)
        {
            Console.WriteLine("Result: " + r.Record.Text);
            Console.WriteLine("Score: " + r.Score);
        }
    }

    string[] lines =
    [
        //"Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase. It serves as an efficient middleware that enables rapid delivery of enterprise-grade solutions.",
        //"Semantic Kernel is a new AI SDK, and a simple and yet powerful programming model that lets you add large language capabilities to your app in just a matter of minutes. It uses natural language prompting to create and execute semantic kernel AI tasks across multiple languages and platforms.",
        //"A dog is an animal that barks. A cat is an animal that meows. A bird is an animal that chirps. A fish is an animal that swims. A horse is an animal that neighs. A cow is an animal that moos. A pig is an animal that oinks. A sheep is an animal that baas. A goat is an animal that bleats. A chicken is an animal that clucks.",
        """
        έχει τα παρακάτω προσόντα
        Ημ. γεννήσεως: 11/07/2000
        Πόλη: ΠΑΤΡΑ
        TK: 26223
        Περιφερειακή ενότητα:
        Νομός: Αχαΐας
        Εκπαίδευση
        Επιλέξτε το ανώτερο επίπεδο εκπαίδευσής σας: ΠΕ (Πανεπιστημιακή εκπαίδευση)
        Πανεπιστημιακή εκπαίδευση (ΠΕ)
        Κατηγορία πτυχίου ΠΕ: ΠΕ ΙΤΑΛΙΚΗΣ ΓΛΩΣΣΑΣ ΚΑΙ ΦΙΛΟΛΟΓΙΑΣ
        Τίτλος πτυχίου ΠΕ: ΠΕ Ιταλικής Γλώσσας και Φιλολογίας
        Προέλευση πτυχίου ΠΕ: ΠΑΝΕΠΙΣΤΗΜΙΟ ΕΛΛΑΔΟΣ
        Βαθμός πτυχίου ΠΕ: 7,09
        Έτος κτήσης πτυχίου ΠΕ: 2023
        Αντιγραφό πτυχίου ΠΕ: Λήψη
        Δημόσια εκπαίδευση (ΔΕ)
        Κατηγορία πτυχίου ΔΕ: ΔΕ ΔΙΟΙΚΗΤΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΟΥ – ΛΟΓΙΣΤΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΟΥ – ΟΙΚΟΝΟΜΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΩΝ ΓΡΑΜΜΑΤΕΩΝ
        Προέλευση πτυχίου ΔΕ: ΓΕΝΙΚΟ ΛΥΚΕΙΟ
        Βαθμός πτυχίου ΔΕ: 16,6
        Έτος κτήσης πτυχίου ΔΕ: 2018
        Αντιγραφό πτυχίου ΔΕ: Λήψη
        Διαθέτετε παιδαγωγική επάρκεια;: Ναι
        Αντίγραφο παιδαγωγικής επάρκειας: Λήψη
        Ξένες γλώσσες & Υπολογιστές
        Υπάρχει ξένη γλώσσα;: Ναι
        Περιγραφή ξένης γλώσσας: ΑΓΓΛΙΚΑ
        Επίπεδο ξένης γλώσσας: B2: Καλή γνώση
        Προέλευση ξένης γλώσσας: Πιστοποίηση
        Αντιγραφό πτυχίου ξένης γλώσσας: Λήψη
        Υπάρχει 2η ξένη γλώσσα;: Ναι
        Περιγραφή 2ης ξένης γλώσσας: ΙΤΑΛΙΚΑ
        Επίπεδο 2ης ξένης γλώσσας: C2: Άριστη γνώση
        Προέλευση 2ης ξένης γλώσσας: Πιστοποίηση
        Αντιγραφό πτυχίου 2ης ξένης γλώσσας: Λήψη
        Αντιγραφό πτυχίου 2ης ξένης γλώσσας: Λήψη
        Υπάρχει πτυχίο υπολογιστών;: Ναι
        Προέλευση πτυχίου υπολογιστών: Πιστοποίηση
        Επίπεδο πτυχίου υπολογιστών: 3 Ενότητες (Word/Excel/Internet)
        Αντιγραφό πτυχίου υπολογιστών: Λήψη
        Έχετε προϋπηρεσία ΙΚΑ μετά το 2002;: Ναι
        Αντίγραφο ΕΦΚΑ: Λήψη
        Άδειες οδήγησης
        Διαθέτετε αδεια οδήγησης αυτοκινήτου (B);: Ναι
        Ημερομηνία λήξης άδειας οδήγησης: 18/12/2034
        Αντιγραφό αδείας: Λήψη
        Οικογενειακή κατάσταση
        Η οικογενειακή σας κατάσταση είναι...: Άγαμος
        """,

        """
        έχει τα παρακάτω προσόντα
        Ημ. γεννήσεως: 30/07/1974
        Διεύθυνση: ΛΕΩΦΑΝΤΟΥΣ 4
        Πόλη: ΠΑΤΡΑ
        TK: 26335
        Περιφερειακή ενότητα:
        Νομός: Αχαΐας
        Εκπαίδευση
        Επιλέξτε το ανώτερο επίπεδο εκπαίδευσής σας: ΔΕ (Απόφοιτος Λυκείου οποιοδήποτε τύπου)
        Δημόσια εκπαίδευση (ΔΕ)
        Κατηγορία πτυχίου ΔΕ: ΔΕ ΔΙΟΙΚΗΤΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΟΥ – ΛΟΓΙΣΤΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΟΥ – ΟΙΚΟΝΟΜΙΚΟΥ/ΔΙΟΙΚΗΤΙΚΩΝ ΓΡΑΜΜΑΤΕΩΝ
        Προέλευση πτυχίου ΔΕ: ΓΕΝΙΚΟ ΛΥΚΕΙΟ
        Βαθμός πτυχίου ΔΕ: 19
        Έτος κτήσης πτυχίου ΔΕ: 2018
        Αντιγραφό πτυχίου ΔΕ: Λήψη
        Ξένες γλώσσες & Υπολογιστές
        Υπάρχει ξένη γλώσσα;: Ναι
        Περιγραφή ξένης γλώσσας: ΑΓΓΛΙΚΑ
        Επίπεδο ξένης γλώσσας: C2: Άριστη γνώση
        Προέλευση ξένης γλώσσας: Πιστοποίηση
        Υπάρχει πτυχίο υπολογιστών;: Ναι
        Προέλευση πτυχίου υπολογιστών: Πιστοποίηση
        Επίπεδο πτυχίου υπολογιστών: 3 Ενότητες (Word/Excel/Internet)
        Αντιγραφό πτυχίου υπολογιστών: Λήψη
        Έχετε προϋπηρεσία ΙΚΑ μετά το 2002;: Ναι
        Αντίγραφο ΕΦΚΑ: Λήψη
        Έχετε Προϋπηρεσία ΙΚΑ πρίν το 2002;: Ναι
        Αντίγραφο ΕΦΚΑ: Λήψη
        Οικογενειακή κατάσταση
        Η οικογενειακή σας κατάσταση είναι...: Έγγαμος
        Πιστοποιητικό οικογενειακής κατάστασης: Λήψη
        Είστε τέκνο πολύτεκνης οικογένειας;: Ναι
        Ημερομηνία λήξης πολυτεκνίας: 20/06/2025
        ΑΣΠΕ πατρικό: Λήψη
        Οικογενειακή κατάσταση πατρική: Λήψη
        """
        //"Semantic Kernel (SK) is a lightweight SDK that lets you mix conventional programming languages, like C# and Python, with the latest in Large Language Model (LLM) AI “prompts” with prompt templating, chaining, and planning capabilities.",
        //"Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase. It serves as an efficient middleware that enables rapid delivery of enterprise-grade solutions. Enterprise ready.",
        //"With Semantic Kernel, you can easily build agents that can call your existing code.This power lets you automate your business processes with models from OpenAI, Azure OpenAI, Hugging Face, and more! We often get asked though, “How do I architect my solution?” and “How does it actually work?”"
    ];

    public sealed class DataModel
    {
        [VectorStoreRecordKey]
        [TextSearchResultName]
        public Guid Key { get; init; }

        [VectorStoreRecordData]
        [TextSearchResultValue]
        public string Text { get; init; }

        [VectorStoreRecordData]
        [TextSearchResultLink]
        public string Link { get; init; }

        [VectorStoreRecordData(IsFilterable = true)]
        public required string Tag { get; init; }

        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> Embedding { get; init; }
    }
}