using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SharedConfig;

public static class Conf
{
    public static void Init<T>() where T : class
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<T>() // Loads secrets.json
            .Build();

        Conf.OpenAI.ApiKey = config["OpenAI:ApiKey"];
        Conf.AzureOpenAI.DeploymentName = config["AzureOpenAI:DeploymentName"];
        Conf.AzureOpenAI.Endpoint = config["AzureOpenAI:Endpoint"];
        Conf.AzureOpenAI.ApiKey = config["AzureOpenAI:ApiKey"];
        Conf.AzureAIFoundry.DeploymentName = config["AzureAIFoundry:DeploymentName"];
        Conf.AzureAIFoundry.Endpoint = config["AzureAIFoundry:Endpoint"];
        Conf.AzureAIFoundry.ApiKey = config["AzureAIFoundry:ApiKey"];
        Conf.GoogleTextSearch.SearchEngineId = config["GoogleTextSearch:SearchEngineId"];
        Conf.GoogleTextSearch.ApiKey = config["GoogleTextSearch:ApiKey"];
        Conf.ApplicationInsights.ConnectionString = config["ApplicationInsights:ConnectionString"];
    }

    public static class OpenAI
    {
        public static string ApiKey;
    }

    public static class AzureOpenAI
    {
        public static string DeploymentName;
        public static string Endpoint;
        public static string ApiKey;
    }

    public static class AzureAIFoundry
    {
        public static string DeploymentName;
        public static string Endpoint;
        public static string ApiKey;
    }

    public static class GoogleTextSearch
    {
        public static string SearchEngineId;
        public static string ApiKey;
    }

    public static class ApplicationInsights
    {
        public static string ConnectionString;
    }
}

public class RunDirectlyAttribute : Attribute
{
}

public interface ITest
{
    Task Run() => Task.CompletedTask;
}

public interface ITestBuilder : ITest
{
    Task Run(IServiceProvider serviceProvider);
    // void Build(IServiceCollection services, ILoggerFactory factory);
}