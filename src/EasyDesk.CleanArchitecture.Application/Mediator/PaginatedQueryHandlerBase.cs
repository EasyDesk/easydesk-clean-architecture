using EasyDesk.CleanArchitecture.Application.Pages;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public abstract class PaginatedQueryHandlerBase<TRequest, TResponse> : RequestHandlerBase<TRequest, Page<TResponse>>
        where TRequest : PaginatedQueryBase<TResponse>
    {
    }
}
