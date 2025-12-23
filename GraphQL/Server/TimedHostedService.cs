using HotChocolate.Subscriptions;
using Microsoft.Extensions.Hosting;
using Server.Types;

namespace Server;

public class TimedHostedService : IHostedService, IDisposable
{
    private readonly ILogger<TimedHostedService> _logger;
    private Timer? _timer;
    
    private readonly ITopicEventSender _sender;

    public TimedHostedService(ILogger<TimedHostedService> logger, ITopicEventSender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Timed Hosted Service is working. Current time: {Time}", DateTimeOffset.Now);

        await _sender.SendAsync<Item>(nameof(Subscription.ItemUpdated),
            new Item(DateTime.UtcNow.ToLongTimeString()));

        await _sender.SendAsync<Item>("harry",
            new Item(DateTime.UtcNow.ToLongTimeString()));

        await _sender.SendAsync<Item>("ExampleTopic",
            new Item(DateTime.UtcNow.ToLongTimeString()));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}