namespace EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;

public interface IQueryHandler<TQuery, TResult> : ICqrsRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}
