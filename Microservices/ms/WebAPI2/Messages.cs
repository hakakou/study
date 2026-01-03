using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoContracts;

namespace WebAPI2;

public class GettingStartedConsumer(ILogger<GettingStartedConsumer> logger) : IConsumer<GettingStarted>
{
    public Task Consume(ConsumeContext<GettingStarted> context)
    {
        logger.LogInformation("Received Text: {Text}", context.Message.Message);
        return Task.CompletedTask;
    }
}