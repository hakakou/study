using Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StrawberryShake.Extensions;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

// Register services
builder.Services
    .AddMyClient()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7260/graphql"))
    .ConfigureWebSocketClient(c => c.Uri = new Uri("wss://localhost:7260/graphql"));

var host = builder.Build();

Console.WriteLine("Client is running...");

IServiceProvider services = host.Services;

IMyClient client = services.GetRequiredService<IMyClient>();


var m = await client.Query.ExecuteAsync();
Console.WriteLine(m.Dump());

var subscription = client.DataAdded.Watch().Subscribe(result =>
{
    if (result.Data != null)
    {
        Console.WriteLine("Data received: "+ result.Data.DataAdded.Name);
    }
    
    if (result.Errors != null && result.Errors.Count > 0)
    {
        foreach (var error in result.Errors)
        {
            Console.WriteLine($"Error: {error.Message}");
        }
    }
});

await host.RunAsync();


subscription.Dispose();