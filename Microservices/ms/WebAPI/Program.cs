using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<TestService>();

var mongoConnectionString = builder.Configuration.GetConnectionString("webdb")!;
builder.Services.AddDbContext<PlanetDbContext>(options =>
{
    options.UseMongoDB(mongoConnectionString, "webdb");
});


builder.Services.AddOptions<SqlTransportOptions>().Configure(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("webdb2")!;
});
builder.Services.AddPostgresMigrationHostedService();
builder.Services.AddMassTransit(x =>
{
    x.UsingPostgres((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});


builder.Services.Configure<MyOptions>(builder.Configuration.GetSection("MyOptions"));
builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(30));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // https://localhost:7128/openapi/v1.json
    app.MapOpenApi().CacheOutput();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


// https://github.com/Redocly/redoc
// Swashbuckle.AspNetCore.ReDoc
app.UseReDoc(options =>
{
    options.DocumentTitle = "WebAPI API Docs";
    options.RoutePrefix = "docs";
    options.SpecUrl("/openapi/v1.json");
});


await app.EnsureDbSeeded();

app.Run();

public class MyOptions
{
    public string Option1 { get; set; }
}

namespace DemoContracts
{
    // Copy of Messages.cs
    public record GettingStarted
    {
        public string Message { get; init; }
    }
}

public class TestService(IBus _eventBus, ILogger<TestService> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug($"Background is starting.");

        // Registers a delegate that will be called when this token is canceled.
        stoppingToken.Register(() =>
            _logger.LogDebug($"Background task is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug($"GracePeriod task doing background work.");
            // ... do background work here
            await _eventBus.Publish(new DemoContracts.GettingStarted
            {
                Message = "Hello from TestService!"
            });
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (TaskCanceledException exception)
            {
                _logger.LogCritical(exception, "Background task delay was cancelled.");
            }
        }

        _logger.LogDebug($"Background task completed.");
    }
}