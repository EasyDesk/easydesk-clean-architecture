using EasyDesk.CleanArchitecture.Application.Cqrs.Operations;
using EasyDesk.CleanArchitecture.Application.Cqrs.Queries;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Data;

public class UnitOfWorkStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public UnitOfWorkStep(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        if (request is IQuery)
        {
            return await next();
        }
        return await _unitOfWorkProvider.RunTransactionally(() => next());
    }
}
