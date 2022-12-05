using EasyDesk.CleanArchitecture.Application.Dispatching;
using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class DispatchingMessageHandler<T> : IHandleMessages<T>
    where T : IDispatchable<Nothing>
{
    private readonly IDispatcher _dispatcher;

    public DispatchingMessageHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(T message) => await _dispatcher.Dispatch(message).ThenThrowIfFailure();
}
