namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface IQueryHandler<TRequest, TResponse> : IRequestHandlerWithResponse<TRequest, TResponse>
    where TRequest : QueryBase<TResponse>
{
}
