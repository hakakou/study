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

        Conf.AzureFoundry.DeploymentName = config["AzureFoundry:DeploymentName"];
        Conf.AzureFoundry.Endpoint = config["AzureFoundry:Endpoint"];
        Conf.AzureFoundry.ApiKey = config["AzureFoundry:ApiKey"];
        Conf.AzureFoundry.EmbeddingDeploymentName = config["AzureFoundry:EmbeddingDeploymentName"];

        Conf.GoogleTextSearch.SearchEngineId = config["GoogleTextSearch:SearchEngineId"];
        Conf.GoogleTextSearch.ApiKey = config["GoogleTextSearch:ApiKey"];
        Conf.ApplicationInsights.ConnectionString = config["ApplicationInsights:ConnectionString"];
    }

    public static class OpenAI
    {
        public static string ApiKey;
    }

    public static class AzureFoundry
    {
        public static string DeploymentName;
        public static string EmbeddingDeploymentName;
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
