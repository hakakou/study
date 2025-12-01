using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SharedConfig;

public static class Conf
{
    public const int DefaultEmbeddingDimension = 3072;

    public static void Init<T>() where T : class
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<T>() // Loads secrets.json
            .Build();

        Conf.OpenAI.ApiKey = config["OpenAI:ApiKey"];

        Conf.AzureFoundry.DeploymentName = config["AzureFoundry:DeploymentName"];
        Conf.AzureFoundry.Endpoint = config["AzureFoundry:Endpoint"];
        Conf.AzureFoundry.ApiKey = config["AzureFoundry:ApiKey"];

        Conf.AzureFoundryEmbeddings.DeploymentName = config["AzureFoundryEmbeddings:DeploymentName"];
        Conf.AzureFoundryEmbeddings.Endpoint = config["AzureFoundryEmbeddings:Endpoint"];
        Conf.AzureFoundryEmbeddings.ApiKey = config["AzureFoundryEmbeddings:ApiKey"];

        Conf.AzureAnthropic.DeploymentName = config["AzureAnthropic:DeploymentName"];
        Conf.AzureAnthropic.Endpoint = config["AzureAnthropic:Endpoint"];
        Conf.AzureAnthropic.ApiKey = config["AzureAnthropic:ApiKey"];

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
        public static string Endpoint;
        public static string ApiKey;
    }

    public static class AzureAnthropic
    {
        public static string DeploymentName;
        public static string Endpoint;
        public static string ApiKey;
    }

    public static class AzureFoundryEmbeddings
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