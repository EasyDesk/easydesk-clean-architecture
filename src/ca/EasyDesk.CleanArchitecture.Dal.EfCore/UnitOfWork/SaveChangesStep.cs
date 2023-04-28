using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

public delegate Task SaveChangesDelegate();

public sealed class SaveChangesStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly SaveChangesDelegate _saveChangesDelegate;

    public SaveChangesStep(SaveChangesDelegate saveChangesDelegate)
    {
        _saveChangesDelegate = saveChangesDelegate;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();

        await _saveChangesDelegate();

        return result;
    }
}
