﻿using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;
using Rebus.Handlers;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class DispatchingMessageHandler<T> : IHandleMessages<T>
    where T : IIncomingMessage
{
    private readonly IDispatcher _dispatcher;

    public DispatchingMessageHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(T message)
    {
        var result = await _dispatcher.Dispatch(message);
        result.ThrowIfFailure(e => new ResultFailedException($"Error while dispatching request of type '{message.GetType().FullName}' ({message}): {e}", e));
    }
}
