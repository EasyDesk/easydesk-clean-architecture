using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Tasks;
using Rebus.Bus;
using Rebus.Exceptions;
using Rebus.Handlers;
using Rebus.Retry.Simple;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class FailedMessageHandler<T> : IHandleMessages<IFailed<T>>
    where T : IIncomingMessage
{
    private readonly IEnumerable<IFailureStrategy> _failureStrategies;
    private readonly IBus _bus;

    public FailedMessageHandler(IEnumerable<IFailureStrategy> failureStrategies, IBus bus)
    {
        _failureStrategies = failureStrategies;
        _bus = bus;
    }

    public async Task Handle(IFailed<T> message)
    {
        try
        {
            var pipeline = _failureStrategies.FoldRight<IFailureStrategy, AsyncAction>(
                () => _bus.Advanced.TransportMessage.Deadletter(),
                (strategy, rest) => () => strategy.Handle(message, rest));

            await pipeline();
        }
        catch (Exception e)
        {
            await _bus.Advanced.TransportMessage.Deadletter();
        }
    }
}
