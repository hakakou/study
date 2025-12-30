using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace Showcase.Tests;

public class MediatorTests(ITestOutputHelper helper)
{
    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITestOutputHelper>(helper);
        services.AddMediator(
            (MediatorOptions options) =>
            {
                options.Assemblies = [typeof(Ping)];
                //options.NotificationPublisherType = typeof(FireAndForgetNotificationPublisher);
                //options.NotificationPublisherType = typeof(Mediator.TaskWhenAllPublisher);
                options.PipelineBehaviors =
                [
                    // Ordering of pipeline behavior registrations matter!
                    typeof(ErrorLoggerHandler<,>),
                    typeof(PingValidator),
                ];
            }
        );

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Send_ValidPing_ReturnsCorrectPong()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var ping = new Ping(Guid.NewGuid());

        // Act
        var pong = await mediator.Send(ping);

        // Assert
        Assert.Equal(ping.Id, pong.Id);
    }

    [Fact]
    public async Task Send_Ping1_MissingMessageHandlerException()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var ping = new Ping1(Guid.NewGuid());

        await Assert.ThrowsAsync<MissingMessageHandlerException>(
            () => mediator.Send(ping).AsTask());
    }

    [Fact]
    public async Task Send_Ping2_MissingMessageHandlerException()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var ping = new Ping2(Guid.NewGuid());
        var pong = await mediator.Send(ping);

        // Ping2a_Handler is called, Ping2b_Handler is ignored with a warning 
    }

    [Fact]
    public async Task Send_InvalidPing_ThrowsArgumentException()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var ping = new Ping(default);

        // Act & Assert
        // PingValidator should throw ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(() => mediator.Send(ping).AsTask());
    }

    [Fact]
    public async Task Notifications_TrackStatsCorrectly()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var statsHandler = sp.GetRequiredService<StatsNotificationHandler>();

        var ping1 = new Ping(Guid.NewGuid());
        await mediator.Send(ping1);

        // Second ping fails validation
        var ping2 = new Ping(default);
        try
        {
            await mediator.Send(ping2);
        }
        catch (ArgumentException)
        {
            // Expected exception
        }

        var (messageCount, messageErrorCount) = statsHandler.Stats;

        // Assert
        Assert.Equal(2, messageCount);
        Assert.Equal(1, messageErrorCount);
    }

    [Fact]
    public async Task Publish_TestNotification()
    {
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var testNotification = new TestNotification("Test data");

        // Can be 0 or more handlers for TestNotification
        await mediator.Publish(testNotification);
    }

    [Fact]
    public async Task Integration_FullScenario_WorksAsExpected()
    {
        // Arrange
        var sp = BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();
        var statsHandler = sp.GetRequiredService<StatsNotificationHandler>();

        // Act & Assert
        // Test successful ping
        var ping = new Ping(Guid.NewGuid());
        var pong = await mediator.Send(ping);
        Assert.Equal(ping.Id, pong.Id);

        // Test validation failure
        ping = ping with { Id = default };
        await Assert.ThrowsAsync<ArgumentException>(() => mediator.Send(ping).AsTask());

        // Verify stats
        var (messageCount, messageErrorCount) = statsHandler.Stats;
        Assert.Equal(2, messageCount);
        Assert.Equal(1, messageErrorCount);
    }
}
