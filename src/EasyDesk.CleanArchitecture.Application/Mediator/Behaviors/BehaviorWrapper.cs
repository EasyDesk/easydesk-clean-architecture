using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;

/// <summary>
/// A wrapper for pipeline behaviors on requests extending from <see cref="ICqrsRequest{T}"/>.
/// This is required since ASP.NET core DI container doesn't fully support open generics,
/// therefore the real behavior would not be correctly detected by MediatR.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class BehaviorWrapper<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        return await CreateBehaviorIfPossible().Match(
            some: behavior => behavior.Handle(request, cancellationToken, next),
            none: () => next());
    }

    private Option<IPipelineBehavior<TRequest, TResponse>> CreateBehaviorIfPossible()
    {
        try
        {
            var responseType = typeof(TResponse);
            var openResponseType = responseType.GetGenericTypeDefinition();
            var wrappedType = responseType.GetGenericArguments().First();
            return Some(CreateBehavior(typeof(TRequest), wrappedType));
        }
        catch
        {
            return None;
        }
    }

    protected abstract IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType);
}
