using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using NodaTime;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public delegate Option<Duration> BackoffStrategy(int deferCount);

public class FailedMessageHandler<T> : IHandleMessages<IFailed<T>>
{
    private readonly IBus _bus;
    private readonly BackoffStrategy _backoffStrategy;

    public FailedMessageHandler(IBus bus, BackoffStrategy backoffStrategy)
    {
        _bus = bus;
        _backoffStrategy = backoffStrategy;
    }

    public async Task Handle(IFailed<T> message)
    {
        var deferCount = message.Headers.GetOption(Headers.DeferCount).Map(int.Parse).OrElse(0);
        await _backoffStrategy(deferCount).MatchAsync(
            some: delay => _bus.Advanced.TransportMessage.Defer(delay.ToTimeSpan()),
            none: () => _bus.Advanced.TransportMessage.Deadletter($"Failed after {deferCount} deferrals\n\n{message.ErrorDescription}"));
    }
}
