using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Tasks;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class ScheduledRetriesStrategy : IFailureStrategy
{
    private readonly IBus _bus;
    private readonly BackoffStrategy _backoffStrategy;

    public ScheduledRetriesStrategy(IBus bus, BackoffStrategy backoffStrategy)
    {
        _bus = bus;
        _backoffStrategy = backoffStrategy;
    }

    public async Task Handle<T>(IFailed<T> message, AsyncAction next) where T : IIncomingMessage
    {
        var deferCount = message.Headers.GetOption(Headers.DeferCount).Map(int.Parse).OrElse(0);
        var delay = _backoffStrategy(deferCount);
        await delay.MatchAsync(
            some: delay => _bus.Advanced.TransportMessage.Defer(delay.ToTimeSpan()),
            none: () => next());
    }
}
