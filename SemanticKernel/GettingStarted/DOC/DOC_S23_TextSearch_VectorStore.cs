using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Npgsql;
using Qdrant.Client;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0050

public class DOC_S23_TextSearch_VectorStore(
    Kernel kernel, IEmbeddingGenerator<string, Embedding<float>> embeddingClient) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion()
            .DefaultEmbeddings();
        services.AddLogging(c =>
          c.AddConsole().SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        Console.WriteLine("=== Vector Store Text Search Demo ===\n");

        // Setup: Create and populate the vector store collection
        var collection = await SetupVectorStoreCollectionAsync();

        await Example1_BasicTextSearchAsync(collection);
        await Example2_BasicTextSearchAsync(collection);
        await Example1_HybridTextSearchAsync(collection);
        await Example2_TextSearchInPromptAsync(collection);
        await Example3_FunctionCallingAsync(collection);
        await Example4_CustomSearchFunctionAsync(collection);
        await Example5_CustomMappersAsync(collection);
    }

    /// <summary>
    /// Example 1: Basic text search returning TextSearchResult items
    /// </summary>
    private async Task Example1_BasicTextSearchAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 1: Basic Text Search");

        var textSearch = new VectorStoreTextSearch<MyModel>(collection, embeddingClient);

        var query = "What is music?";

        KernelSearchResults<TextSearchResult> textResults =
            await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });

        Console.WriteLine($"Query: {query}\n");
        await foreach (var result in textResults.Results)
        {
            Console.WriteLine($"Name:  {result.Name}");
            Console.WriteLine($"Value: {result.Value}");
            Console.WriteLine($"Link:  {result.Link}");
            Console.WriteLine(new string('-', 60));
        }
    }

    private async Task Example2_BasicTextSearchAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 1: Basic Text Search");

        var query = "What is sound?";
        var searchEmbedding = await embeddingClient.GenerateAsync(query);

        var vectorSearchOptions = new VectorSearchOptions<MyModel>
        {
            //Filter = r => r.Text == "food",
            Skip = 0,
            IncludeVectors = true,
        };

        var searchResult = collection.SearchAsync(searchEmbedding, top: 3, vectorSearchOptions);

        Console.WriteLine($"Query: {query}\n");
        await foreach (var result in searchResult)
        {
            Console.WriteLine($"Name:  {result.Record.Text}");
            Console.WriteLine($"Value: {result.Score:F4}");
            Console.WriteLine($"Tag:  {result.Record.Tag}");
            Console.WriteLine(new string('-', 60));
        }
    }

    private async Task Example1_HybridTextSearchAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 1: Hybrid Text Search");

        var hybridSearchableCollection = collection as IKeywordHybridSearchable<MyModel>;

        var searchEmbedding = await embeddingClient.GenerateAsync("What is science?");

        // Perform hybrid search: combines vector similarity with keyword matching
        // The keywords help find documents that contain specific terms
        // while the vector ensures semantic relevance
        var searchResult = hybridSearchableCollection.HybridSearchAsync(
            searchEmbedding.Vector,
            [], top: 3);

        // Iterate over the search results
        await foreach (var result in searchResult)
        {
            Console.WriteLine($"Hotel: {result.Record.Text}");
            Console.WriteLine($"Description: {result.Record.Tag}");
            Console.WriteLine($"Score: {result.Score:F4}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Example 2: Using text search in a prompt with Handlebars template
    /// </summary>
    private async Task Example2_TextSearchInPromptAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 2: Text Search in Prompt");

        // Create a text search instance using the vector store collection
        var textSearch = new VectorStoreTextSearch<MyModel>(collection, embeddingClient);

        // Build a text search plugin and add to the kernel
        var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(searchPlugin);

        string promptTemplate = """
            {{#with (SearchPlugin-GetTextSearchResults query)}}  
                {{#each this}}  
                Name: {{Name}}
                Value: {{Value}}
                Link: {{Link}}
                -----------------
                {{/each}}  
            {{/with}}  

            {{query}}

            Include citations to the relevant information where it is referenced in the response.
            """;

        KernelArguments arguments = new() { { "query", "Who can work as a programmer?" } };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();

        var response = await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: promptTemplateFactory
        );
        Console.WriteLine(response);

        // Clean up
        kernel.Plugins.Clear();
    }

    /// <summary>
    /// Example 3: Using vector store with automatic function calling
    /// </summary>
    private async Task Example3_FunctionCallingAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 3: Automatic Function Calling");

        // Create a text search instance using the vector store collection
        var textSearch = new VectorStoreTextSearch<MyModel>(collection, embeddingClient);

        // Build a text search plugin with vector store search and add to the kernel
        var searchPlugin = textSearch.CreateWithGetTextSearchResults("WorkPlugin");
        kernel.Plugins.Add(searchPlugin);

        // Invoke prompt and use text search plugin to provide grounding information
        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        KernelArguments arguments = new(settings);

        var query = "Who can work as a programmer? Search using the WorkPlugin";
        Console.WriteLine($"Query: {query}\n");
        var response = await kernel.InvokePromptAsync(query, arguments);
        Console.WriteLine(response);

        // Clean up
        kernel.Plugins.Clear();
    }

    private static KernelFunction CreatePagedSearchFunction(ITextSearch textSearch)
    {
        async Task<IEnumerable<TextSearchResult>> SearchAsync(
            Kernel kernel, KernelFunction function, KernelArguments arguments,
            CancellationToken cancellationToken, int count = 5, int skip = 0)
        {
            arguments.TryGetValue("query", out var queryObj);
            var query = queryObj?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query))
            {
                return [];
            }

            // Create *fresh* options each time so count/skip are honored
            var searchOptions = new TextSearchOptions
            {
                Top = count,
                Skip = skip
            };

            var result = await textSearch
                .GetTextSearchResultsAsync(query, searchOptions, cancellationToken)
                .ConfigureAwait(false);

            return await result.Results.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        var options = new KernelFunctionFromMethodOptions
        {
            FunctionName = "Search",
            Description = "Search CVs of candidates.",
            Parameters =
            [
                new KernelParameterMetadata("query")
            {
                Description = "What to search for",
                IsRequired  = true
            },
            new KernelParameterMetadata("count")
            {
                Description  = "Number of results",
                IsRequired   = false, DefaultValue = 5
            },
            new KernelParameterMetadata("skip")
            {
                Description  = "Number of results to skip",
                IsRequired   = false, DefaultValue = 0
            },
            ],
            ReturnParameter = new() { ParameterType = typeof(IEnumerable<TextSearchResult>) },
        };

        return KernelFunctionFactory.CreateFromMethod(SearchAsync, options);
    }

    /// <summary>
    /// Example 4: Customizing the search function with metadata
    /// </summary>
    private async Task Example4_CustomSearchFunctionAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 4: Custom Search Function");

        // Create a text search instance using the vector store collection
        ITextSearch textSearch = new VectorStoreTextSearch<MyModel>(collection, embeddingClient);

        // Does not work as expected - caching of options causes skip/count to be ignored
        // NotWorking(textSearch);

        var searchFunction = CreatePagedSearchFunction(textSearch);

        var searchPlugin = KernelPluginFactory.CreateFromFunctions(
            "SearchPlugin", "Search CVs of candidates", [searchFunction]);
        kernel.Plugins.Add(searchPlugin);

        // Use automatic function calling
        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        KernelArguments arguments = new(settings);

        var query = "Get the people who can work as a programmer? Use SearchPlugin and search for max 1 at a time (use the skip until no good candidates are returned)";
        Console.WriteLine($"Query: {query}\n");
        var response = await kernel.InvokePromptAsync(query, arguments);
        Console.WriteLine(response);

        // Clean up
        kernel.Plugins.Clear();


        static void NotWorking(VectorStoreTextSearch<MyModel> textSearch)
        {
            // Create options to describe the function
            var options = new KernelFunctionFromMethodOptions()
            {
                FunctionName = "Search",
                Description = "Search CVs of candidates.",
                Parameters =
                [
                    new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                    new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true, DefaultValue = 2 },
                    // This is ignored because of caching in the function created
                    new KernelParameterMetadata("skip") { Description = "Number of results to skip", IsRequired = true, DefaultValue = 0 },
                ],
                ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
            };

            var searchFunction = textSearch.CreateGetTextSearchResults(options);
            var searchPlugin = KernelPluginFactory.CreateFromFunctions(
                "SearchPlugin", "Search CVs of candidates",
                [searchFunction]);
        }
    }

    /// <summary>
    /// Example 5: Using custom mappers for data model transformation
    /// </summary>
    private async Task Example5_CustomMappersAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        Utils.PrintSectionHeader("Example 5: Custom Mappers");

        // Create custom mappers
        var stringMapper = new DataModelTextSearchStringMapper();
        var resultMapper = new DataModelTextSearchResultMapper();

        var textSearch = new VectorStoreTextSearch<MyModel>(collection, embeddingClient,
            stringMapper, resultMapper);

        // Search and return results
        var query = "What is the Semantic Kernel?";
        KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(
            query,
            new TextSearchOptions { Top = 2 });

        Console.WriteLine($"Query: {query}\n");
        Console.WriteLine("Results using custom mappers:\n");
        await foreach (TextSearchResult result in textResults.Results)
        {
            Console.WriteLine($"Name:  {result.Name}");
            Console.WriteLine($"Value: {result.Value}");
            Console.WriteLine($"Link:  {result.Link}");
            Console.WriteLine(new string('-', 60));
        }
    }

    async Task<VectorStore> DefaultQdrantVectorStoreAsync()
    {
        return new QdrantVectorStore(
            new QdrantClient("localhost"), ownsClient: true,
            new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingClient });
    }

    async Task<VectorStore> DefaultPostgresVectorStoreAsync()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("Host=localhost;Port=5432;Database=mydb;Username=admin;Password=admin");
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        // Ensure the vector extension is created
        // This seems to fail the first time.
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE EXTENSION IF NOT EXISTS vector;";
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        var vectorStore = new PostgresVectorStore(
            dataSource, ownsDataSource: true,
            new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingClient });

        return vectorStore;
    }

    private async Task<VectorStoreCollection<Guid, MyModel>> SetupVectorStoreCollectionAsync()
    {
        var vectorStore = await DefaultQdrantVectorStoreAsync();
        // var vectorStore = await DefaultPostgresVectorStoreAsync();
        var collection = vectorStore.GetCollection<Guid, MyModel>("sk_textsearch_demo2");

        // Create the collection if it doesn't exist
        await collection.EnsureCollectionExistsAsync();

        // Populate with sample data
        //await PopulateSampleDataAsync(collection);

        return collection;
    }

    /// <summary>
    /// Populate the collection with sample data about professional profiles
    /// </summary>
    private async Task PopulateSampleDataAsync(VectorStoreCollection<Guid, MyModel> collection)
    {
        var samples = new[]
        {
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000001"),
                Text = "Profile: John Smith. Education: Bachelor's degree in Computer Science from MIT (2018), Master's degree in Artificial Intelligence from Stanford University (2020). Certifications: AWS Solutions Architect, Kubernetes Administrator. Work Experience: Senior Software Engineer at Google (3 years), Full Stack Developer at Microsoft (2 years), Junior Developer at StartupXYZ (1 year). Skills: Python, C#, Java, Cloud Architecture, Machine Learning, Microservices.",
                Link = "https://linkedin.com/profile/john-smith",
                Tag = "engineer"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000002"),
                Text = "Profile: Sarah Johnson. Education: Bachelor's degree in Business Administration from Harvard University (2017), MBA in Finance from Wharton School (2019). Certifications: CFA Level 3, PMP (Project Management Professional). Work Experience: Financial Analyst at Goldman Sachs (4 years), Investment Manager at BlackRock (2 years), Business Consultant at McKinsey & Company (3 years). Expertise: Portfolio Management, Risk Analysis, Strategic Planning, Financial Modeling.",
                Link = "https://linkedin.com/profile/sarah-johnson",
                Tag = "finance"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000003"),
                Text = "Profile: Michael Chen. Education: Doctor of Medicine (MD) from Johns Hopkins University (2015), Bachelor's degree in Biology from UC Berkeley (2011). Certifications: Board Certified in Internal Medicine, Advanced Life Support (ALS). Work Experience: Attending Physician at Mayo Clinic (5 years), Medical Resident at Cleveland Clinic (3 years), Clinical Intern at Stanford Medical Center (1 year). Specializations: Cardiology, Patient Care Management, Medical Research, Clinical Diagnosis.",
                Link = "https://linkedin.com/profile/michael-chen",
                Tag = "healthcare"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000004"),
                Text = "Profile: Emily Rodriguez. Education: Bachelor's degree in Architecture from Carnegie Mellon University (2016), Master's degree in Urban Planning from MIT (2018). Certifications: LEED Accredited Professional, Autodesk Certified Associate. Work Experience: Senior Architect at Foster + Partners (3 years), Architectural Designer at Zaha Hadid Architects (2 years), Junior Architect at David Chipperfield Architects (2 years). Focus Areas: Sustainable Design, Urban Development, 3D Modeling, Project Management.",
                Link = "https://linkedin.com/profile/emily-rodriguez",
                Tag = "architecture"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000005"),
                Text = "Profile: David Park. Education: Bachelor's degree in Marketing from Northwestern University (2017), Diploma in Digital Marketing from Google Academy (2019). Certifications: Google Analytics Certified, HubSpot Inbound Marketing Certified, Facebook Blueprint Certified. Work Experience: Digital Marketing Manager at Amazon (3 years), Marketing Specialist at Adobe (2 years), Content Strategist at Buzzfeed (1 year). Core Competencies: SEO, Data Analytics, Campaign Management, Brand Strategy, Content Marketing.",
                Link = "https://linkedin.com/profile/david-park",
                Tag = "marketing"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000006"),
                Text = "Profile: Alexandra Thompson. Education: Bachelor's degree in Mechanical Engineering from Cal Tech (2016), Master's degree in Aerospace Engineering from MIT (2018). Certifications: Professional Engineer (PE) License, Six Sigma Black Belt. Work Experience: Lead Aerospace Engineer at SpaceX (4 years), Systems Engineer at Boeing (3 years), Design Engineer at Lockheed Martin (2 years). Expertise: Spacecraft Design, Propulsion Systems, Quality Assurance, Manufacturing Process Optimization.",
                Link = "https://linkedin.com/profile/alexandra-thompson",
                Tag = "engineering"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000007"),
                Text = "Profile: Robert Wilson. Education: Bachelor's degree in Law from Yale University (2012), JD from Columbia Law School (2015). Certifications: Bar Admission (New York and California), Certified Legal Specialist in Corporate Law. Work Experience: Senior Attorney at Sullivan & Cromwell (5 years), Partner at Davis & Associates (3 years), Associate at White & Case (2 years). Practice Areas: M&A Law, Corporate Transactions, Contract Negotiation, Regulatory Compliance.",
                Link = "https://linkedin.com/profile/robert-wilson",
                Tag = "legal"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000008"),
                Text = "Profile: Lisa Anderson. Education: Bachelor's degree in Nursing from Johns Hopkins University (2013), Master's degree in Healthcare Administration from Harvard University (2016). Certifications: Registered Nurse (RN), Certified Nursing Leader (CNL), Advanced Cardiac Life Support (ACLS). Work Experience: Nurse Manager at Massachusetts General Hospital (4 years), Clinical Nurse Specialist at Brigham and Women's Hospital (3 years), Staff Nurse at Boston Medical Center (2 years). Areas of Expertise: Patient Management, Team Leadership, Quality Improvement, Clinical Operations.",
                Link = "https://linkedin.com/profile/lisa-anderson",
                Tag = "nursing"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000009"),
                Text = "Profile: James Martinez. Education: Bachelor's degree in Civil Engineering from UC San Diego (2015), Master's degree in Project Management from Arizona State University (2018). Certifications: PMP (Project Management Professional), Certified Associate Project Manager (CAPM), LEED AP. Work Experience: Project Manager at Turner Construction (4 years), Civil Engineer at Bechtel Corporation (3 years), Construction Coordinator at PCL Construction (2 years). Specializations: Infrastructure Development, Budget Management, Stakeholder Communication, Construction Scheduling.",
                Link = "https://linkedin.com/profile/james-martinez",
                Tag = "construction"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000010"),
                Text = "Profile: Victoria Lee. Education: Bachelor's degree in Data Science from UC Berkeley (2017), Master's degree in Statistics from Stanford University (2019). Certifications: Google Cloud Data Engineer, AWS Certified Data Analytics Specialist. Work Experience: Senior Data Scientist at Meta (3 years), Data Analytics Engineer at Netflix (2 years), Junior Data Scientist at Uber (1 year). Technical Skills: Python, R, SQL, Machine Learning, Big Data Analysis, Data Visualization, Deep Learning.",
                Link = "https://linkedin.com/profile/victoria-lee",
                Tag = "data-science"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000011"),
                Text = "Profile: Christopher Brown. Education: Bachelor's degree in Environmental Science from University of Washington (2014), PhD in Environmental Engineering from UC Davis (2019). Certifications: LEED AP BD+C, Professional Scientist (PScD). Work Experience: Environmental Consultant at McKinsey & Company (3 years), Senior Environmental Engineer at AECOM (2 years), Research Scientist at Berkeley Lab (2 years). Expertise: Sustainability Assessment, Environmental Impact Analysis, Green Infrastructure, Climate Change Mitigation.",
                Link = "https://linkedin.com/profile/christopher-brown",
                Tag = "environment"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000012"),
                Text = "Profile: Michelle Garcia. Education: Bachelor's degree in Psychology from University of Florida (2016), Master's degree in Clinical Psychology from NYU (2018). Certifications: Licensed Clinical Professional Counselor (LCPC), Cognitive Behavioral Therapy (CBT) Specialist. Work Experience: Senior Therapist at Mindful Wellness Center (3 years), Clinical Psychologist at New York Presbyterian Hospital (2 years), Counselor at Family Services Agency (2 years). Specializations: Mental Health Counseling, Trauma Therapy, Family Dynamics, Behavioral Analysis.",
                Link = "https://linkedin.com/profile/michelle-garcia",
                Tag = "psychology"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000013"),
                Text = "Profile: Thomas Kim. Education: Bachelor's degree in Accounting from University of Pennsylvania (2015), Master's of Business Taxation from Boston University (2017). Certifications: Certified Public Accountant (CPA), Certified Management Accountant (CMA), Enrolled Agent (EA). Work Experience: Senior Accountant at Deloitte (4 years), Tax Manager at PwC (3 years), Accounting Specialist at KPMG (2 years). Areas of Focus: Tax Planning, Financial Auditing, Corporate Accounting, Regulatory Compliance.",
                Link = "https://linkedin.com/profile/thomas-kim",
                Tag = "accounting"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000014"),
                Text = "Profile: Jennifer White. Education: Bachelor's degree in Human Resources Management from Michigan State University (2014), Master's degree in Organizational Development from University of Minnesota (2017). Certifications: Society for Human Resource Management (SHRM-SCP), Certified Professional in Human Resources (PHR), Executive Coach Certification. Work Experience: VP of Human Resources at Apple Inc. (4 years), HR Manager at Intel Corporation (3 years), Recruiter at Salesforce (2 years). Core Competencies: Talent Management, Organizational Strategy, Employee Development, Compensation & Benefits.",
                Link = "https://linkedin.com/profile/jennifer-white",
                Tag = "human-resources"
            },
            new
            {
                Key = new Guid("00000000-0000-0000-0000-000000000015"),
                Text = "Profile: Marcus Johnson. Education: Bachelor's degree in Electrical Engineering from Georgia Tech (2016), Master's degree in Power Systems from MIT (2019). Certifications: Professional Engineer (PE) in Electrical Engineering, IEEE Senior Member. Work Experience: Lead Electrical Engineer at Tesla (3 years), Power Systems Engineer at Duke Energy (2 years), Design Engineer at Siemens Energy (2 years). Specializations: Renewable Energy Systems, Smart Grid Technology, High Voltage Systems, Energy Efficiency Solutions.",
                Link = "https://linkedin.com/profile/marcus-johnson",
                Tag = "energy"
            }
        };

        foreach (var sample in samples)
        {
            // Generate embedding for the text
            var embedding = await embeddingClient.GenerateAsync(sample.Text);

            // Upsert the record
            await collection.UpsertAsync(new MyModel
            {
                Key = sample.Key,
                Text = sample.Text,
                Link = sample.Link,
                Tag = sample.Tag,
                Embedding = embedding.Vector
            });
        }
    }

    /// <summary>
    /// Data model with text search result attributes for declarative mapping
    /// </summary>
    public sealed class MyModel
    {
        [VectorStoreKey]
        [TextSearchResultName]
        public Guid Key { get; init; }

        [VectorStoreData(IsFullTextIndexed = true)]
        [TextSearchResultValue]
        public required string Text { get; init; }

        [VectorStoreData]
        [TextSearchResultLink]
        public required string Link { get; init; }

        [VectorStoreData(IsIndexed = true)]
        public required string Tag { get; init; }

        [VectorStoreVector(Dimensions: 3072)]
        public ReadOnlyMemory<float> Embedding { get; init; }
    }

    /// <summary>
    /// String mapper which converts a DataModel to a string.
    /// </summary>
    private sealed class DataModelTextSearchStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result)
        {
            if (result is MyModel dataModel)
            {
                return dataModel.Text;
            }
            throw new ArgumentException("Invalid result type.");
        }
    }

    /// <summary>
    /// Result mapper which converts a DataModel to a TextSearchResult.
    /// </summary>
    private sealed class DataModelTextSearchResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result)
        {
            if (result is MyModel dataModel)
            {
                return new TextSearchResult(value: dataModel.Text)
                {
                    Name = dataModel.Key.ToString(),
                    Link = dataModel.Link
                };
            }
            throw new ArgumentException("Invalid result type.");
        }
    }
}
