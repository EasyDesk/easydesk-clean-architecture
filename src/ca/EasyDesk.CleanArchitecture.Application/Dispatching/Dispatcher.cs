using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using EasyDesk.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

internal class Dispatcher : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _dispatchMethodsByType = [];
    private readonly ILifetimeScope _lifetimeScope;
    private readonly bool _nested;

    public Dispatcher(ILifetimeScope lifetimeScope) : this(lifetimeScope, false)
    {
    }

    private Dispatcher(ILifetimeScope lifetimeScope, bool nested)
    {
        _lifetimeScope = lifetimeScope;
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
        await using var scope = _nested
            ? _lifetimeScope.BeginLifetimeScope()
            : _lifetimeScope.BeginUseCaseLifetimeScope(builder =>
            {
                builder.Register(c => new Dispatcher(c.Resolve<ILifetimeScope>(), nested: true))
                    .As<IDispatcher>()
                    .InstancePerDependency();
            });
        var handler = FindHandler<T, X>(scope);
        var pipeline = scope.Resolve<IPipelineProvider>().GetSteps<T, R>(scope);

        if (_nested)
        {
            pipeline = pipeline.Where(step => step.IsForEachHandler);
        }
        return await pipeline.Run(dispatchable, r => handler.Handle(r).ThenMapAsync(mapper));
    }

    private IHandler<T, R> FindHandler<T, R>(ILifetimeScope scope)
        where T : IDispatchable<R>
    {
        return scope
            .ResolveOption<IHandler<T, R>>()
            .OrElseThrow(() => new HandlerNotFoundException(typeof(T)));
    }
}
