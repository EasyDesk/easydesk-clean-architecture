using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Mapping;

public abstract class MappingHandler<T, F, R> : IHandler<T, R>
    where T : IDispatchable<R>
    where R : IMappableFrom<F, R>
{
    public Task<Result<R>> Handle(T request) => Process(request).ThenMap(R.MapFrom);

    protected abstract Task<Result<F>> Process(T request);
}
