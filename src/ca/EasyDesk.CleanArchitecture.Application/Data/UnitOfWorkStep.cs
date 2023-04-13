using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Data;

public sealed class UnitOfWorkStep<T, R> : IPipelineStep<T, R>
    where R : notnull
    where T : IReadWriteOperation
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public UnitOfWorkStep(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _unitOfWorkProvider.RunTransactionally(() => next());
    }
}
