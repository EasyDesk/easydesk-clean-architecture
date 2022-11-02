using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
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

    public async Task<Result<T>> Dispatch<T>(IDispatchable<T> request)
    {
        var requestType = request.GetType();
        var methodInfo = _dispatchMethodsByType.GetOrAdd(requestType, t => typeof(Dispatcher)
            .GetMethod(nameof(DispatchImpl), BindingFlags.NonPublic | BindingFlags.Instance)
            .MakeGenericMethod(t, typeof(T)));
        return await (Task<Result<T>>)methodInfo.Invoke(this, new[] { request });
    }

    private async Task<Result<R>> DispatchImpl<T, R>(T request)
        where T : IDispatchable<R>
    {
        return await _pipeline.GetSteps<T, R>()
            .Reverse()
            .Aggregate(Handler<T, R>(request), (next, step) => () => step.Run(request, next))();
    }

    private NextPipelineStep<R> Handler<T, R>(T request)
        where T : IDispatchable<R>
    {
        return () => FindHandler<T, R>().Handle(request);
    }

    private IHandler<T, R> FindHandler<T, R>()
        where T : IDispatchable<R>
    {
        var handler = _serviceProvider.GetService<IHandler<T, R>>();
        return handler ?? throw new HandlerNotFoundException(typeof(T));
    }
}
