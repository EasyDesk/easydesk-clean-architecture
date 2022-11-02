using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Messaging;
using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class DispatchingMessageHandler<T> : IHandleMessages<T>
    where T : IDispatchable<Nothing>, IMessage
{
    private readonly IDispatcher _dispatcher;

    public DispatchingMessageHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(T message) => await _dispatcher.Dispatch(message).ThenThrowIfFailure();
}
