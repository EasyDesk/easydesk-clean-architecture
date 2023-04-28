namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public interface IHandler<T, R>
    where T : IDispatchable<R>
{
    Task<Result<R>> Handle(T request);
}

public interface IHandler<T> : IHandler<T, Nothing>
    where T : IDispatchable<Nothing>
{
}
