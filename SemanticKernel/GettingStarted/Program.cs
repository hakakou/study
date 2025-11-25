using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedConfig;
using Spectre.Console;
using System.Reflection;
using static SharedConfig.Conf;

#pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

public class Program
{
    public static ServiceProvider ServiceProvider;

    private static async Task Main(string[] args)
    {
        Conf.Init<Program>();

        var collection = new ServiceCollection();

        var testTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(ITest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderByDescending(q => q.Name)
            .ToList();
        foreach (var type in testTypes)
            collection.AddTransient(type);

        ServiceProvider = collection.BuildServiceProvider();

        var directRunType = testTypes.FirstOrDefault(t => t.GetCustomAttribute<RunDirectlyAttribute>() != null);
        if (directRunType != null)
        {
            var test = (ServiceProvider.GetRequiredService(directRunType) as ITest)!;
            await test.Run();
        }
        else
        {
            var selectedFunction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a function to run")
                    .AddChoices(testTypes.Select(t => t.Name).ToArray()));

            var selectedType = testTypes.FirstOrDefault(t => t.Name == selectedFunction);
            if (selectedType != null)
            {
                var test = (ServiceProvider.GetRequiredService(selectedType) as ITest)!;
                await test.Run();
            }
        }

        Console.WriteLine();
        Console.WriteLine();
    }
}

public interface ITest
{
    Task Run();
}

