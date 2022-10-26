using EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;
using EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Cqrs;

internal class CqrsRequestDispatcher : ICqrsRequestDispatcher
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> _dispatchMethodsByType = new();
    private readonly IServiceProvider _serviceProvider;

    public CqrsRequestDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<TResult>> Dispatch<TResult>(ICqrsRequest<TResult> request)
    {
        var requestType = request.GetType();
        var methodInfo = _dispatchMethodsByType.GetOrAdd(requestType, t => typeof(CqrsRequestDispatcher)
            .GetMethod(nameof(DispatchImpl), BindingFlags.NonPublic | BindingFlags.Instance)
            .MakeGenericMethod(t, typeof(TResult)));
        return await (Task<Result<TResult>>)methodInfo.Invoke(this, new[] { request });
    }

    private async Task<Result<TResult>> DispatchImpl<TRequest, TResult>(TRequest request)
        where TRequest : ICqrsRequest<TResult>
    {
        var steps = _serviceProvider.GetServices<IPipelineStep<TRequest, TResult>>();
        return await steps
            .Reverse()
            .Aggregate(Handler<TRequest, TResult>(request), (next, step) => () => step.Run(request, next))();
    }

    private NextPipelineStep<TResult> Handler<TRequest, TResult>(TRequest request)
        where TRequest : ICqrsRequest<TResult>
    {
        return () => FindHandler<TRequest, TResult>().Handle(request);
    }

    private ICqrsRequestHandler<TRequest, TResult> FindHandler<TRequest, TResult>()
        where TRequest : ICqrsRequest<TResult>
    {
        var handler = _serviceProvider.GetService<ICqrsRequestHandler<TRequest, TResult>>();
        return handler ?? throw new HandlerNotFoundException(typeof(TRequest));
    }
}
