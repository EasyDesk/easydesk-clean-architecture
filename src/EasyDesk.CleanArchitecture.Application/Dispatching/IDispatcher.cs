namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public interface IDispatcher
{
    Task<Result<T>> Dispatch<T>(IDispatchable<T> dispatchable);
}
