using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Data;

public class SaveChangesStep<T, R> : IPipelineStep<T, R>
{
    private readonly ISaveChangesHandler _saveChangesHandler;

    public SaveChangesStep(ISaveChangesHandler saveChangesHandler)
    {
        _saveChangesHandler = saveChangesHandler;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        return await next().ThenIfSuccessAsync(_ => _saveChangesHandler.SaveChanges());
    }
}
