namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public interface IDispatcher
{
    Task<Result<R>> Dispatch<R>(IDispatchable<R> dispatchable);
}
