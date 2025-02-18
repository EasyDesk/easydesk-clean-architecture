using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

internal class Dispatcher : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _dispatchMethodsByType = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly bool _nested;

    public Dispatcher(IServiceProvider serviceProvider) : this(serviceProvider, false)
    {
    }

    private Dispatcher(IServiceProvider serviceProvider, bool nested)
    {
        _serviceProvider = serviceProvider;
        _nested = nested;
    }

    public async Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper)
    {
        var dispatchableType = dispatchable.GetType();
        var methodInfo = _dispatchMethodsByType
            .GetOrAdd(dispatchableType, t => typeof(Dispatcher)
                .GetMethod(nameof(DispatchImpl), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(t, typeof(X), typeof(R)));
        return await (Task<Result<R>>)methodInfo.Invoke(this, [dispatchable, mapper])!;
    }

    private async Task<Result<R>> DispatchImpl<T, X, R>(T dispatchable, AsyncFunc<X, R> mapper)
        where T : IDispatchable<X>
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var handler = FindHandler<T, X>(serviceProvider);
        var pipeline = serviceProvider.GetRequiredService<IPipelineProvider>().GetSteps<T, R>(serviceProvider);

        if (_nested)
        {
            pipeline = pipeline.Where(step => step.IsForEachHandler);
        }
        return await pipeline.Run(dispatchable, r => handler.Handle(r).ThenMapAsync(mapper));
    }

    private IHandler<T, R> FindHandler<T, R>(IServiceProvider serviceProvider)
        where T : IDispatchable<R>
    {
        return serviceProvider
            .GetServiceAsOption<IHandler<T, R>>()
            .OrElseThrow(() => new HandlerNotFoundException(typeof(T)));
    }
}
