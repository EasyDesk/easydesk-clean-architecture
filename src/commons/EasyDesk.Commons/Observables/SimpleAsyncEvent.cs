﻿using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons.Observables;

public sealed class SimpleAsyncEvent<T> : IAsyncObservable<T>, IAsyncEmitter<T>
{
    private readonly IList<AsyncAction<T>> _handlers = [];

    public async Task Emit(T value)
    {
        foreach (var handler in _handlers)
        {
            await handler(value);
        }
    }

    public ISubscription Subscribe(AsyncAction<T> handler)
    {
        _handlers.Add(handler);
        return new SimpleSubscription(() => _handlers.Remove(handler));
    }
}
