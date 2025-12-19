using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;
using MiniExcelLibs;
using MiniExcelLibs.Attributes;
using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using OpenQA.Selenium;

public class App(Kernel kernel, ITextSearch textSearch, IWebDriver driver)
{
    public async Task Execute()
    {
        var websearchPlugin = KernelPluginFactory.CreateFromFunctions(
            "WebSearch", "Search using Google", [CreateSearchBySite((TavilyTextSearch)textSearch)]);
        kernel.Plugins.Add(websearchPlugin);

        var linkedinPlugin = KernelPluginFactory.CreateFromObject(
            new LinkedinSearch(driver), "LinkedinSearch");
        // kernel.Plugins.Add(linkedinPlugin);

        kernel.AutoFunctionInvocationFilters.Add(new StopOnSearchRateLimitFilter());

        // Invoke prompt and use text search plugin to provide grounding information
        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ResponseFormat = typeof(ResultData)
        };

        var templateConfig = new PromptTemplateConfig()
        {
            Name = "PersonLinkedInSearch",
            Template = """
            Use the WebSearch plugin to try to find the LinkedIn profile for the person below.
            Best results would be EU based with a connection to the EU and especially
            the European Commission DIGIT department (Directorate-General for Informatics).

            Person Information:
            - Name: {{$Surname}}
            - Title: {{$Title}}
            - City: {{$City}}
            - Nationality: {{$Nationality}}
            - External: {{$External}}

            Field Descriptions:
            - Title: Courtesy title (Mr/Ms)
            - Surname: Person full name (often 'SURNAME / NAME')
            - City: Duty station/city code (e.g., BRU=Brussels, LUX=Luxembourg)
            - Nationality: Nationality as a 2-letter country code (may be empty)
            - External: Is the person external to the DIGIT department ("external" or empty if not external)

            Search for their LinkedIn profile (only on site www.linkedin.com) and provide:
            1. LinkedIn URL
            2. Certainty level (1-5) of the match
            3. If certainty is low, provide an alternative LinkedIn URL
            4. Company name from their profile (leave empty if not found)
            5. Location from their profile (leave empty if not found)
            6. Profession/job title from their profile (leave empty if not found)
            7. Education details from their profile (leave empty if not found)
            8. Any interesting notes (up to 120 characters)

            How to search:
            - Try a search with query "NAME site:linkedin.com european commission".
            - If no match again, think of common variations on the name (especially for spanish names) 
              and try searching again.
            - You are free to use the two plugins with variations to find the best match.
            """,
            TemplateFormat = "semantic-kernel",
            InputVariables = [
                new() { Name = "Surname", Description = "Person full name", IsRequired = true },
                new() { Name = "Title", Description = "Courtesy title (Mr/Ms)", IsRequired = false },
                new() { Name = "City", Description = "Duty station/city code", IsRequired = false },
                new() { Name = "Nationality", Description = "Nationality code", IsRequired = false },
                new() { Name = "External", Description = "Is the person external to the European Commission?", IsRequired = false }
            ]
        };

        //var templateConfig = new PromptTemplateConfig()
        //{
        //    Name = "PersonLinkedInSearch",
        //    Template = """
        //    Use the LinkedinSearch plugin to find the LinkedIn profile for the person below.
        //    Best results would be EU based with a connection to the EU and especially
        //    the European Commission DIGIT department (Directorate-General for Informatics).

        //    Person Information:
        //    - Name: {{$Surname}}
        //    - Title: {{$Title}}
        //    - City: {{$City}}
        //    - Nationality: {{$Nationality}}
        //    - External: {{$External}}

        //    Field Descriptions:
        //    - Title: Courtesy title (Mr/Ms)
        //    - Surname: Person full name (often 'SURNAME / NAME')
        //    - City: Duty station/city code (e.g., BRU=Brussels, LUX=Luxembourg)
        //    - Nationality: Nationality as a 2-letter country code (may be empty)
        //    - External: Is the person external to the DIGIT department ("external" or empty if not external)

        //    Search for their LinkedIn profile and provide:
        //    1. LinkedIn URL
        //    2. Certainty level (1-5) of the match
        //    3. Company name from their profile (leave empty if not found)
        //    4. Location from their profile (leave empty if not found)
        //    5. Profession/job title from their profile (leave empty if not found)
        //    6. Education details from their profile (leave empty if not found)
        //    7. Any interesting notes (up to 120 characters)

        //    How to search:
        //    - Use the LinkedinSearch.Search function to find potential matches for the person's name.
        //    - If no results, you may also retry variations of the name (especially for Spanish names with compound surnames).
        //    - From the search results:
        //      * If you can identify a good match directly from the search results, select that profile.
        //      * Else open possible profiles (up to 3 total) using LinkedinSearch.OpenProfile. 
        //      * Start from the top results to avoid opening more profiles than needed.
        //    - Select the profile that best matches based on:
        //      * Location (preference for Brussels, Luxembourg, or other EU cities)
        //      * Company/organization (preference for European Commission, EU institutions, or DIGIT or EU related work)
        //      * Professional background matching the person's role
        //    """,
        //    TemplateFormat = "semantic-kernel",
        //    InputVariables = [
        //        new() { Name = "Surname", Description = "Person full name", IsRequired = true },
        //        new() { Name = "Title", Description = "Courtesy title (Mr/Ms)", IsRequired = false },
        //        new() { Name = "City", Description = "Duty station/city code", IsRequired = false },
        //        new() { Name = "Nationality", Description = "Nationality code", IsRequired = false },
        //        new() { Name = "External", Description = "Is the person external to the European Commission?", IsRequired = false }
        //    ]
        //};

        var f = @"c:\Unzip\app\DIGIT_people_output.xlsx";
        var latest = @"c:\Unzip\app\DIGIT_people_latest.xlsx";

        var rows = MiniExcel.Query<ExcelRow>(f).ToList();

        var rowsToProcess = rows.Where(r => r.Certainty == 0).ToList();
        var totalRows = rowsToProcess.Count;

        await AnsiConsole.Progress().Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Processing LinkedIn profiles[/]", maxValue: totalRows);

                foreach (var row in rowsToProcess)
                {
                    task.Description = $"[green]Processing:[/] {row.Surname}";

                    var arguments = new KernelArguments(settings)
                    {
                        ["Surname"] = row.Surname,
                        ["Title"] = row.Title,
                        ["City"] = row.City,
                        ["Nationality"] = row.Nationality,
                        ["External"] = row.External
                    };

                    try
                    {
                        var r = await kernel.InvokePromptAsync<string>(templateConfig.Template, arguments);
                        var result = JsonSerializer.Deserialize<ResultData>(r);

                        row.LinkedInUrl = result.LinkedInUrl;
                        row.Certainty = result.Certainty;
                        row.Company = result.Company;
                        row.Location = result.Location;
                        row.Profession = result.Profession;
                        row.Education = result.Education;
                        row.Notes = result.Notes;

                        var markup = new Markup($"[yellow]{row.Surname.Trim().EscapeMarkup()}[/] " +
                            $"{row.Notes.Trim().EscapeMarkup()} ([white]{row.Certainty}[/])");
                        AnsiConsole.Write(markup);
                        AnsiConsole.WriteLine();

                        task.Increment(1);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error processing {row.Surname}:[/] {ex.Message}");
                        break;
                    }

                    MiniExcel.SaveAs(f, rows, overwriteFile: true);

                    try
                    {
                        // Make a copy of the latest file after each row is processed for viewing.
                        // Ignore errors (in case the file is open).
                        File.Copy(f, latest, true);
                    }
                    catch { }
                }

                task.Description = $"[green]Completed processing {totalRows} rows[/]";
            });
    }

    private static KernelFunction CreateSearchBySite(TavilyTextSearch textSearch)
    {
        var options = new KernelFunctionFromMethodOptions()
        {
            FunctionName = "Search",
            Description = "Perform a search for content related to the specified query and optionally from the specified domain.",
            Parameters =
            [
                new KernelParameterMetadata("query") { Description = "What to search for", IsRequired = true },
                new KernelParameterMetadata("count") { Description = "Number of results", IsRequired = true },
                new KernelParameterMetadata("include_domain") { Description = "Domain", IsRequired = false },
            ],
            ReturnParameter = new() { ParameterType = typeof(KernelSearchResults<string>) },
        };

        return textSearch.CreateGetTextSearchResults(options, new TextSearchOptions() { Top = 10 });
    }
}


public class ExcelRow : ResultData
{
    [ExcelColumn(Name = "unit")]
    public string Unit { get; set; }

    [ExcelColumn(Name = "external")]
    public string External { get; set; }

    [ExcelColumn(Name = "Mr / Ms")]
    public string Title { get; set; }

    [ExcelColumn(Name = "SURNAME / NAME")]
    public string Surname { get; set; }

    [ExcelColumn(Name = "city")]
    public string City { get; set; }

    [ExcelColumn(Name = "FC")]
    public string FC { get; set; }

    [ExcelColumn(Name = "NATIONALITY")]
    public string Nationality { get; set; }
}


public class ResultData
{
    public string LinkedInUrl { get; set; }

    [Description("Certainty level of the LinkedIn URL (1-5).")]
    public int Certainty { get; set; }

    public string Company { get; set; }

    public string Location { get; set; }

    public string Profession { get; set; }

    public string Education { get; set; }

    public string Notes { get; set; }
}
