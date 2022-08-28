namespace EasyDesk.CleanArchitecture.Application.Cqrs;

public interface ICqrsRequestDispatcher
{
    Task<Result<TResult>> Dispatch<TResult>(ICqrsRequest<TResult> request);
}
