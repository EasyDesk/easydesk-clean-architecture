using EasyDesk.CleanArchitecture.Application.Data;

namespace EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;

public class UnitOfWorkStep<TRequest, TResult> : IPipelineStep<TRequest, TResult>
    where TRequest : ICommand<TResult>
{
    private readonly IUnitOfWorkProvider _unitOfWorkProvider;

    public UnitOfWorkStep(IUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public async Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next)
    {
        return await _unitOfWorkProvider.RunTransactionally(() => next());
    }
}
