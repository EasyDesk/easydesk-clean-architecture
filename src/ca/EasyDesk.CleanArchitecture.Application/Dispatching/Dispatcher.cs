﻿using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

internal class Dispatcher : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _dispatchMethodsByType = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IPipeline _pipeline;

    public Dispatcher(IServiceProvider serviceProvider, IPipeline pipeline)
    {
        _serviceProvider = serviceProvider;
        _pipeline = pipeline;
    }

    public async Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper)
    {
        var dispatchableType = dispatchable.GetType();
        var methodInfo = _dispatchMethodsByType
            .GetOrAdd(dispatchableType, t => typeof(Dispatcher)
                .GetMethod(nameof(DispatchImpl), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(t, typeof(X), typeof(R)));
        return await (Task<Result<R>>)methodInfo.Invoke(this, new object[] { dispatchable, mapper })!;
    }

    private async Task<Result<R>> DispatchImpl<T, X, R>(T dispatchable, AsyncFunc<X, R> mapper)
        where T : IDispatchable<X>
    {
        var handler = FindHandler<T, X>();
        return await _pipeline
            .GetSteps<T, R>(_serviceProvider)
            .Reverse()
            .Aggregate<IPipelineStep<T, R>, NextPipelineStep<R>>(
                () => handler.Handle(dispatchable).ThenMapAsync(mapper),
                (next, step) => () => step.Run(dispatchable, next))();
    }

    private IHandler<T, R> FindHandler<T, R>()
        where T : IDispatchable<R>
    {
        var handler = _serviceProvider.GetService<IHandler<T, R>>();
        return handler ?? throw new HandlerNotFoundException(typeof(T));
    }
}
