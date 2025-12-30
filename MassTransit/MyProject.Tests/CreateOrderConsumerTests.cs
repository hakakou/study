using Company.Application.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Components.Consumers;
using NUnit.Framework;
using System.Threading.Tasks;

namespace MyProject.Tests;

public class CreateOrderConsumerTests
{
    [Test]
    public async Task Should_create_an_order_and_produce_an_event()
    {
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.DisableUsageTelemetry();
                cfg.AddConsumer<CreateCustomerConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = await provider.StartTestHarness();

        var orderId = NewId.NextGuid();

        await harness.Bus.Publish(new CreateCustomer("a"));

        Assert.That(await harness.Consumed.Any<CreateCustomer>(x => x.Context.Message.Name == "a"));
    }
}