using Mediator;
using Xunit;

namespace Showcase.Tests;

public sealed record Ping(Guid Id) : IRequest<Pong>;

public sealed record Pong(Guid Id);

public sealed class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}

public sealed record Ping1(Guid Id) : IRequest<Pong>;

public sealed record Ping2(Guid Id) : IRequest<Pong>;
public sealed class Ping2a_Handler : IRequestHandler<Ping2, Pong>
{
    public ValueTask<Pong> Handle(Ping2 request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}

// Ignored, see warning generated
public sealed class Ping2b_Handler : IRequestHandler<Ping2, Pong>
{
    public ValueTask<Pong> Handle(Ping2 request, CancellationToken cancellationToken)
    {
        return new ValueTask<Pong>(new Pong(request.Id));
    }
}



public sealed record NoReturn : IRequest<Unit>;

public sealed class NoReturnHandler : IRequestHandler<NoReturn>
{
    public ValueTask<Unit> Handle(NoReturn request, CancellationToken cancellationToken)
    {
        return new ValueTask<Unit>(Unit.Value);
    }
}

public sealed class PingValidator : IPipelineBehavior<Ping, Pong>
{
    public ValueTask<Pong> Handle(
        Ping request,
        MessageHandlerDelegate<Ping, Pong> next,
        CancellationToken cancellationToken
    )
    {
        if (request is null || request.Id == default)
            throw new ArgumentException("Invalid input");

        return next(request, cancellationToken);
    }
}

public sealed record ErrorMessage(Exception Exception) : INotification;

public sealed record SuccessfulMessage() : INotification;

public sealed record TestNotification(string data) : INotification;

public sealed class ErrorLoggerHandler<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    // Constrained to IMessage, or constrain to IBaseCommand or any custom interface you've implemented
{

    private readonly IMediator _mediator;
    private readonly ITestOutputHelper _helper;

    public ErrorLoggerHandler(IMediator mediator, ITestOutputHelper helper)
    {
        _mediator = mediator;
        _helper = helper;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await next(message, cancellationToken);
            await _mediator.Publish(new SuccessfulMessage());
            _helper.WriteLine($"ErrorLoggerHandler: Successfully handled message of type {typeof(TMessage).Name}");
            return response;
        }
        catch (Exception ex)
        {
            await _mediator.Publish(new ErrorMessage(ex));
            _helper.WriteLine($"ErrorLoggerHandler: Error handling message of type {typeof(TMessage).Name}: {ex.Message}");
            throw;
        }
    }
}

public sealed class ErrorNotificationHandler(ITestOutputHelper _helper) : INotificationHandler<ErrorMessage>
{
    public ValueTask Handle(ErrorMessage error, CancellationToken cancellationToken)
    {
        // Could log to application insights or something...
        _helper.WriteLine($"ErrorNotificationHandler: {error.Exception.Message}");
        return default;
    }
}

public sealed class ErrorNotificationHandler2(ITestOutputHelper _helper) : INotificationHandler<ErrorMessage>
{
    public ValueTask Handle(ErrorMessage error, CancellationToken cancellationToken)
    {
        // Could log to application insights or something...
        _helper.WriteLine($"ErrorNotificationHandler2: {error.Exception.Message}");
        return default;
    }
}

public sealed class StatsNotificationHandler : INotificationHandler<INotification>
{
    private long _messageCount;
    private long _messageErrorCount;

    public (long MessageCount, long MessageErrorCount) Stats => (_messageCount, _messageErrorCount);

    public async ValueTask Handle(INotification notification, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _messageCount);
        if (notification is ErrorMessage)
            Interlocked.Increment(ref _messageErrorCount);
    }
}

public sealed class TestNotificationHandler(ITestOutputHelper helper) : INotificationHandler<TestNotification>
{
    public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
    {
        helper.WriteLine($"TestNotificationHandler received data: {notification.data}");
        return default;
    }
}

public sealed class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification
{
    public ValueTask Handle(TNotification notification, CancellationToken cancellationToken)
    {
        return default;
    }
}

public sealed class FireAndForgetNotificationPublisher(ITestOutputHelper helper) : INotificationPublisher
{
    public async ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken
    )
        where TNotification : INotification
    {
        try
        {
            await Task.WhenAll(handlers
                .Select(handler => handler.Handle(notification, cancellationToken).AsTask()));
        }
        catch (Exception)
        {
            helper.WriteLine($"Exception occurred while publishing notification of type {typeof(TNotification).Name}");
            // Notifications should be fire-and-forget, we just need to log it!
            // This way we don't have to worry about exceptions bubbling up when publishing notifications
            // Silently catch in tests
        }
    }
}
