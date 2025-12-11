using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

public class DOC_S15_CreateFunctions(Kernel kernel) : ITest
{
    public static void Build(IServiceCollection services)
    {
        services.AddKernel()
            .DefaultChatCompletion();
        services.AddLogging(c => c.AddConsole()
         .SetMinimumLevel(LogLevel.Trace));
    }

    public async Task Run()
    {
        // Basic prompt template without JSON schema
        var templateConfig = new PromptTemplateConfig
        {
            Name = "PersonGreeting",
            Description = "Generates a personalized greeting based on name and age",
            Template = "Generate one friendly greeting for {{$name}}, appropriate for his age ({{$age}}).",
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() {
                    Name = "name",
                    Description = "The person's name",
                    IsRequired = true,
                },
                new() {
                    Name = "age",
                    Description = "The person's age",
                    IsRequired = true,
                }
            ]
        };

        var function = kernel.CreateFunctionFromPrompt(templateConfig);
        kernel.Plugins.AddFromFunctions("Test", [function]);

        string chatPrompt = """
            <message role="user">Hello!</message>
            <message role="system">{{Test.PersonGreeting}}</message>
            """;

        var factory = new KernelPromptTemplateFactory();
        IPromptTemplate promptTemplate = factory.Create(new PromptTemplateConfig(chatPrompt));

        var args = new KernelArguments()
        {
            ["name"] = "Alice",
            ["age"] = "30"
        };

        var str = await promptTemplate.RenderAsync(kernel, args);
        Console.WriteLine(str);
        return;


        //await Example1_BasicJsonSchemaInput();
        //await Example2_ComplexJsonSchemaInput();
        await Example3_JsonSchemaOutput();
        //await Example4_YamlPromptWithJsonSchema();
        //await Example5_CombinedInputOutputJsonSchema();
    }

    /// <summary>
    /// Example 1: Basic JSON Schema for Input Variables
    /// Demonstrates how to define simple JSON schemas for input validation.
    /// </summary>
    private async Task Example1_BasicJsonSchemaInput()
    {
        Utils.PrintSectionHeader("=== Example 1: Basic JSON Schema for Input Variables ===\n");

        // Define a simple JSON schema for a string input with constraints
        var nameSchema = """
            {
                "type": "string",
                "minLength": 2,
                "maxLength": 50,
                "description": "A person's name between 2 and 50 characters"
            }
            """;

        var ageSchema = """
            {
                "type": "integer",
                "minimum": 0,
                "maximum": 150,
                "description": "A person's age between 0 and 150"
            }
            """;

        var templateConfig = new PromptTemplateConfig
        {
            Name = "PersonGreeting",
            Description = "Generates a personalized greeting based on name and age",
            Template = "Generate a friendly greeting for {{$name}} who is {{$age}} years old.",
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() {
                    Name = "name",
                    Description = "The person's name",
                    IsRequired = true,
                    JsonSchema = nameSchema
                },
                new() {
                    Name = "age",
                    Description = "The person's age",
                    IsRequired = true,
                    JsonSchema = ageSchema
                }
            ]
        };

        var function = kernel.CreateFunctionFromPrompt(templateConfig);

        var result = await kernel.InvokeAsync(function, new KernelArguments
        {
            ["name"] = "Alice",
            ["age"] = "30"
        });

        Console.WriteLine($"Result: {result}\n");
    }

    /// <summary>
    /// Example 2: Complex JSON Schema with Objects and Arrays
    /// Shows how to define nested objects and arrays in JSON schemas.
    /// </summary>
    private async Task Example2_ComplexJsonSchemaInput()
    {
        Utils.PrintSectionHeader("=== Example 2: Complex JSON Schema with Objects and Arrays ===\n");

        // Define a complex JSON schema for product information
        var productSchema = """
            {
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Product name"
                    },
                    "price": {
                        "type": "number",
                        "minimum": 0,
                        "description": "Product price in USD"
                    },
                    "categories": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        },
                        "description": "Product categories"
                    },
                    "inStock": {
                        "type": "boolean",
                        "description": "Whether the product is in stock"
                    }
                },
                "required": ["name", "price"]
            }
            """;

        var templateConfig = new PromptTemplateConfig
        {
            Name = "ProductDescription",
            Description = "Generates a marketing description for a product",
            Template = """
                Create a compelling product description for the following product:
                {{$product}}
                
                Make it engaging and highlight key features.
                """,
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() {
                    Name = "product",
                    Description = "Product information in JSON format",
                    IsRequired = true,
                    JsonSchema = productSchema
                }
            ]
        };

        var function = kernel.CreateFunctionFromPrompt(templateConfig);

        // Sample product data
        var productData = new
        {
            name = "SmartWatch Pro",
            price = 299.99,
            categories = new[] { "Electronics", "Wearables", "Fitness" },
            inStock = true
        };

        var result = await kernel.InvokeAsync(function, new KernelArguments
        {
            ["product"] = JsonSerializer.Serialize(productData)
        });

        Console.WriteLine($"Result: {result}\n");
    }

    /// <summary>
    /// Example 3: JSON Schema for Output Variables
    /// Demonstrates how to define expected output structure using JSON schema.
    /// This helps the AI model understand the required format for responses.
    /// </summary>
    private async Task Example3_JsonSchemaOutput()
    {
        Utils.PrintSectionHeader("=== Example 3: JSON Schema for Output Variables ===\n");

        // Define the expected output schema
        var outputSchema = """
            {
                "type": "object",
                "properties": {
                    "summary": {
                        "type": "string",
                        "description": "A brief summary of the article, up to 100 chars."
                    },
                    "keyPoints": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        },
                        "description": "Main points from the article"
                    },
                    "sentiment": {
                        "type": "string",
                        "enum": ["pos", "neg", "neu"],
                        "description": "Overall sentiment of the article"
                    },
                    "confidence": {
                        "type": "number",
                        "description": "Confidence score of the analysis 1-5"
                    }
                },
                "required": ["summary", "keyPoints", "sentiment"]
            }
            """;

        // meta data tha does not work:
        // minItems, maxItems, minimum, maximum


        var templateConfig = new PromptTemplateConfig
        {
            Name = "ArticleAnalyzer",
            Description = "Analyzes an article and returns structured data",
            Template = """
                Analyze the following article and provide a JSON:              
                {{$article}}
                """,
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() {
                    Name = "article",
                    Description = "The article text to analyze",
                    IsRequired = true
                }
            ],
            OutputVariable = new()
            {
                Description = "Structured analysis of the article",
                JsonSchema = outputSchema
            }
        };

        var schema = JsonDocument.Parse(outputSchema).RootElement;

        var sampleArticle = """
            Technology companies continue to invest heavily in artificial intelligence 
            research and development. Recent breakthroughs in language models have 
            demonstrated impressive capabilities in understanding and generating human-like 
            text. Industry experts predict this trend will accelerate innovation across 
            multiple sectors including healthcare, education, and finance.
            """;


        templateConfig.ExecutionSettings = new()
        {
            [PromptExecutionSettings.DefaultServiceId] = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(Summary)

            }
        };

        var function = kernel.CreateFunctionFromPrompt(templateConfig);

        var arguments = new KernelArguments
        {
            ["article"] = sampleArticle,
        };

        var result = await kernel.InvokeAsync(function, arguments);

        Console.WriteLine($"Analysis Result:\n{result}\n");

        var jsonResult = JsonNode.Parse(result.ToString());
        Console.WriteLine("✓ Output successfully parsed as JSON");
        Console.WriteLine($"Summary: {jsonResult?["summary"]?.ToString()}");
        Console.WriteLine($"Sentiment: {jsonResult?["sentiment"]?.ToString()}\n");
        Console.WriteLine($"Confidence: {jsonResult?["confidence"]?.ToString()}\n");
    }

    /// <summary>
    /// Example 4: YAML Prompt with JSON Schema
    /// Shows how to define JSON schemas in YAML prompt configuration files.
    /// This is the preferred approach for complex, reusable prompts.
    /// </summary>
    private async Task Example4_YamlPromptWithJsonSchema()
    {
        Utils.PrintSectionHeader("=== Example 4: YAML Prompt with JSON Schema ===\n");

        var yaml = """
            name: EventPlanner
            template: |
              Plan a {{$eventType}} event with the following details:
              Budget: ${{$budget}}
              Number of guests: {{$guestCount}}
              Location preferences: {{$preferences}}
              
              Provide a detailed event plan including venue suggestions, catering options, 
              and estimated costs.
            template_format: semantic-kernel
            description: Plans an event based on type, budget, and preferences
            input_variables:
              - name: eventType
                description: Type of event (e.g., wedding, corporate, birthday)
                is_required: true
                json_schema: |
                  {
                    "type": "string",
                    "enum": ["wedding", "corporate", "birthday", "conference", "party"],
                    "description": "The type of event to plan"
                  }
              - name: budget
                description: Event budget in USD
                is_required: true
                json_schema: |
                  {
                    "type": "number",
                    "minimum": 100,
                    "maximum": 1000000,
                    "description": "Budget for the event in USD"
                  }
              - name: guestCount
                description: Expected number of guests
                is_required: true
                json_schema: |
                  {
                    "type": "integer",
                    "minimum": 1,
                    "maximum": 10000,
                    "description": "Number of expected guests"
                  }
              - name: preferences
                description: Location and style preferences
                is_required: false
                default: "No specific preferences"
                json_schema: |
                  {
                    "type": "string",
                    "maxLength": 500,
                    "description": "Additional preferences for the event"
                  }
            output_variable:
              description: Detailed event plan with recommendations
              json_schema: |
                {
                  "type": "object",
                  "properties": {
                    "venueSuggestions": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "name": { "type": "string" },
                          "capacity": { "type": "integer" },
                          "estimatedCost": { "type": "number" }
                        }
                      }
                    },
                    "cateringOptions": {
                      "type": "array",
                      "items": { "type": "string" }
                    },
                    "totalEstimatedCost": {
                      "type": "number",
                      "description": "Total estimated cost in USD"
                    }
                  }
                }
            execution_settings:
              default:
                temperature: 0.7
                max_tokens: 2000
            """;

        var function = kernel.CreateFunctionFromPromptYaml(yaml);

        var result = await kernel.InvokeAsync(function, new KernelArguments
        {
            ["eventType"] = "wedding",
            ["budget"] = "50000",
            ["guestCount"] = "150",
            ["preferences"] = "Outdoor venue, elegant atmosphere, near a lake"
        });

        Console.WriteLine($"Event Plan:\n{result}\n");
    }

    /// <summary>
    /// Example 5: Combined Input and Output JSON Schema
    /// Demonstrates a complete scenario with validated inputs and structured outputs.
    /// This is useful for building robust, production-ready AI features.
    /// </summary>
    private async Task Example5_CombinedInputOutputJsonSchema()
    {
        Utils.PrintSectionHeader("=== Example 5: Combined Input and Output JSON Schema ===\n");

        var yaml = """
            name: CustomerSupportTicketAnalyzer
            template: |
              Analyze the following customer support ticket and categorize it:
              
              Ticket ID: {{$ticketId}}
              Customer: {{$customerName}}
              Issue: {{$issueDescription}}
              Priority: {{$priority}}
              
              Provide a structured analysis including category, urgency level, 
              recommended actions, and estimated resolution time.
            template_format: semantic-kernel
            description: Analyzes customer support tickets and provides structured recommendations
            input_variables:
              - name: ticketId
                description: Unique ticket identifier
                is_required: true
                json_schema: |
                  {
                    "type": "string",
                    "pattern": "^TICKET-[0-9]{6}$",
                    "description": "Ticket ID in format TICKET-XXXXXX"
                  }
              - name: customerName
                description: Name of the customer
                is_required: true
                json_schema: |
                  {
                    "type": "string",
                    "minLength": 1,
                    "maxLength": 100
                  }
              - name: issueDescription
                description: Description of the customer's issue
                is_required: true
                json_schema: |
                  {
                    "type": "string",
                    "minLength": 10,
                    "maxLength": 2000,
                    "description": "Detailed description of the issue"
                  }
              - name: priority
                description: Initial priority level
                is_required: true
                json_schema: |
                  {
                    "type": "string",
                    "enum": ["low", "medium", "high", "critical"]
                  }
            output_variable:
              description: Structured ticket analysis with recommendations
              json_schema: |
                {
                  "type": "object",
                  "properties": {
                    "category": {
                      "type": "string",
                      "enum": ["technical", "billing", "product_inquiry", "complaint", "other"],
                      "description": "Issue category"
                    },
                    "urgencyLevel": {
                      "type": "integer",
                      "minimum": 1,
                      "maximum": 5,
                      "description": "Urgency level from 1 (low) to 5 (critical)"
                    },
                    "recommendedActions": {
                      "type": "array",
                      "items": { "type": "string" },
                      "minItems": 1,
                      "description": "List of recommended actions"
                    },
                    "estimatedResolutionHours": {
                      "type": "number",
                      "minimum": 0.5,
                      "description": "Estimated time to resolve in hours"
                    },
                    "requiresEscalation": {
                      "type": "boolean",
                      "description": "Whether the ticket needs escalation"
                    },
                    "suggestedTeam": {
                      "type": "string",
                      "enum": ["support_tier1", "support_tier2", "engineering", "billing", "management"]
                    }
                  },
                  "required": ["category", "urgencyLevel", "recommendedActions", "estimatedResolutionHours", "requiresEscalation", "suggestedTeam"]
                }
            execution_settings:
              default:
                temperature: 0.3
                max_tokens: 1500
            """;

        var function = kernel.CreateFunctionFromPromptYaml(yaml);

        var result = await kernel.InvokeAsync(function, new KernelArguments
        {
            ["ticketId"] = "TICKET-123456",
            ["customerName"] = "John Smith",
            ["issueDescription"] = "I've been charged twice for my subscription this month. " +
                "The payment went through on both my credit card statements but I only " +
                "received one confirmation email. Please help resolve this billing error.",
            ["priority"] = "high"
        });

        Console.WriteLine($"Ticket Analysis:\n{result}\n");

        // Validate and display the structured output
        try
        {
            var analysis = JsonNode.Parse(result.ToString());
            Console.WriteLine("=== Parsed Analysis ===");
            Console.WriteLine($"Category: {analysis?["category"]}");
            Console.WriteLine($"Urgency Level: {analysis?["urgencyLevel"]}");
            Console.WriteLine($"Requires Escalation: {analysis?["requiresEscalation"]}");
            Console.WriteLine($"Suggested Team: {analysis?["suggestedTeam"]}");
            Console.WriteLine($"Estimated Resolution: {analysis?["estimatedResolutionHours"]} hours");

            var actions = analysis?["recommendedActions"]?.AsArray();
            if (actions != null)
            {
                Console.WriteLine("\nRecommended Actions:");
                foreach (var action in actions)
                {
                    Console.WriteLine($"  - {action}");
                }
            }
            Console.WriteLine();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse analysis: {ex.Message}\n");
        }
    }

    public class Summary
    {
        [Description("A brief summary of the article, up to 100 chars.")]
        public string? summary { get; set; }
        public List<string> keyPoints { get; set; }
        public SentimentEnum sentiment { get; set; }
        [Description("Confidence score of the analysis 1-5")]
        public double confidence { get; set; }
    }

    public enum SentimentEnum
    {
        pos,
        neg,
        neu
    }

}
