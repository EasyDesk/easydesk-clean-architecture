using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.Tools.Results;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public abstract record RequestBase<TResponse> : IRequest<Result<TResponse>>;

public abstract record CommandBase<TResponse> : RequestBase<TResponse>;

public abstract record QueryBase<TResponse> : RequestBase<TResponse>;

public abstract record QueryWithPaginationBase<TResponse>(Pagination Pagination) : QueryBase<Page<TResponse>>;
