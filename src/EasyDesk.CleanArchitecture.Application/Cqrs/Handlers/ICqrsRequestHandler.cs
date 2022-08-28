namespace EasyDesk.CleanArchitecture.Application.Cqrs.Handlers;

public interface ICqrsRequestHandler<TRequest, TResult>
    where TRequest : ICqrsRequest<TResult>
{
    Task<Result<TResult>> Handle(TRequest request);
}
