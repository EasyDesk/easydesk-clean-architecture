using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Data;

public sealed class UnitOfWorkStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public UnitOfWorkStep(IUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _unitOfWorkManager.RunTransactionally(() => next());
    }
}
