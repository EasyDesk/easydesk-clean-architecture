using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.CleanArchitecture.Application.Mediator.Handlers;

public interface IQueryWithPaginationHandler<TRequest, TResponse> : IQueryHandler<TRequest, Page<TResponse>>
    where TRequest : IPagedQuery<TResponse>
{
}
