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
    private bool _initialized = false;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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
        DispatcherScopeManager.Current ??= new(_serviceProvider);

        Result<R> result;
        await using (var scope = DispatcherScopeManager.Current.OpenNewScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var handler = FindHandler<T, X>(serviceProvider);
            var pipeline = serviceProvider.GetRequiredService<IPipelineProvider>().GetSteps<T, R>(serviceProvider);
            if (_initialized)
            {
                pipeline = pipeline.Where(step => step.IsForEachHandler);
            }
            else
            {
                _initialized = true;
            }

            result = await pipeline.Run(dispatchable, r => handler.Handle(r).ThenMapAsync(mapper));
        }

        if (DispatcherScopeManager.Current.Depth == 0)
        {
            DispatcherScopeManager.Current = null!;
        }

        return result;
    }

    private IHandler<T, R> FindHandler<T, R>(IServiceProvider serviceProvider)
        where T : IDispatchable<R>
    {
        return serviceProvider
            .GetServiceAsOption<IHandler<T, R>>()
            .OrElseThrow(() => new HandlerNotFoundException(typeof(T)));
    }
}
