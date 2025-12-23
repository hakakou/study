using System;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace Server.Types;

public class Subscription(ILogger<Subscription> logger)
{
    [Subscribe]
    // [Topic("OnSessionUpdated")]
    public Item ItemUpdated([EventMessage] Item item)
    {
        // This is called for each subscriber when the event is triggered
        return new Item("Mod: " + item.Name);
    }

    [Subscribe]
    // The topic argument must be in the format "{argument}"
    // Using string interpolation and nameof is a good way to reference the argument name properly
    [Topic($"{{{nameof(author)}}}")]
    public Item ItemPublished(string author, [EventMessage] Item item)
    {
        return new Item($"Author: {author}, Item: {item.Name}");
    }

    public ValueTask<ISourceStream<Item>> SubscribeToItem(ITopicEventReceiver receiver)
    {
        logger.LogInformation("A new subscription to ExampleTopic has been made.");
        return receiver.SubscribeAsync<Item>("ExampleTopic");
    }

    [Subscribe(With = nameof(SubscribeToItem))]
    public Item DataAdded([EventMessage] Item item) => item ;
}

public record Item(string Name);
