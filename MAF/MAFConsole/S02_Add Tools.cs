using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using System.ComponentModel;

public class S02_AddTools : ITest
{
    public async Task Run()
    {
        var client = new AIProjectClient(
                endpoint: new Uri(Conf.MicrosoftFoundry2.Endpoint),
                tokenProvider: new AzureCliCredential());

        AIAgent agent = client
            .AsAIAgent(
                model: "gpt-4.1",
                tools: [AIFunctionFactory.Create(GetWeather)],
                instructions: "You are a helpful assistant.");

        await foreach (var update in agent.RunStreamingAsync(
            "Compare the weather in Paris and London."))
        {
            Console.Write(update);
        }
    }

    [Description("Get the weather for a given location.")]
    static string GetWeather([Description("The location to get the weather for.")] string location)
        => $"The weather in {location} is cloudy with a high of 15°C.";

    [Description("Get the latest stock price for a ticker symbol.")]
    static decimal GetStockPrice(
        [Description("Ticker symbol, e.g. MSFT.")] string ticker)
        => 425.50m;

    [Description("Send an email to a recipient.")]
    static string SendEmail(
        [Description("Recipient email address.")] string to,
        [Description("Email subject line.")] string subject,
        [Description("Body of the email.")] string body)
        => $"Email sent to {to}.";
}