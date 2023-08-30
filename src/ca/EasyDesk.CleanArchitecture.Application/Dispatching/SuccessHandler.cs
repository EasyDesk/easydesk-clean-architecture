using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Dispatching;

public abstract class SuccessHandler<T, R> : IHandler<T, R>
    where T : IDispatchable<R>
{
    public async Task<Result<R>> Handle(T request) => await Process(request);

    protected abstract Task<R> Process(T request);
}
