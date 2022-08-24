using EasyDesk.CleanArchitecture.Application.Pages;
using MediatR;

namespace EasyDesk.CleanArchitecture.Application.Mediator;

public interface ICqrsRequest<T> : IRequest<Result<T>>
{
}

public interface ICommand<T> : ICqrsRequest<T>
{
}

public interface IQuery<T> : ICqrsRequest<T>
{
}

public interface IPagedQuery<T> : IQuery<Page<T>>
{
}
