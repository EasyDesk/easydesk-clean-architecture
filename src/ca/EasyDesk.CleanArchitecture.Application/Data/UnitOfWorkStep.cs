using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Data;

public sealed class UnitOfWorkStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public UnitOfWorkStep(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public bool IsForEachHandler => false;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await _unitOfWorkProvider.RunTransactionally(() => next());
    }
}
