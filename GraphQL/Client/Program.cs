using Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);

// Register services
builder.Services
    .AddMyClient()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7260/graphql"));

var host = builder.Build();

Console.WriteLine("Client is running...");

IServiceProvider services = host.Services;

IMyClient client = services.GetRequiredService<IMyClient>();

var r = await client.Query.ExecuteAsync();
//Console.WriteLine(r.Dump());

var m = await client.AddItemsToPlaylist.ExecuteAsync(new()
{
    ArticleId = "1",
    Text = "This is a comment from the client."
});
Console.WriteLine(m.Dump());


await host.RunAsync();
