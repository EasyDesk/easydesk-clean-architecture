using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

/// <summary>
/// A wrapper for pipeline behaviors on requests extending from <see cref="RequestBase{T}"/>.
/// This is required since ASP.NET core DI container doesn't fully support open generics,
/// therefore the real behavior would not be correctly detected by MediatR.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class BehaviorWrapper<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : RequestBase<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            var responseType = typeof(TResponse);
            var openResponseType = responseType.GetGenericTypeDefinition();
            var wrappedType = responseType.GetGenericArguments().First();
            var behavior = CreateBehavior(typeof(TRequest), wrappedType);
            return await behavior.Handle(request, cancellationToken, next);
        }
        catch
        {
            return await next();
        }
    }

    protected abstract IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType);
}
