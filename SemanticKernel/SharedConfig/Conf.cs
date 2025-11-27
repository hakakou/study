using Microsoft.Extensions.Configuration;
using System;

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
}

public class RunDirectlyAttribute : Attribute
{
}