using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Sagas;

public class SaveSagaChangesStep<T, R> : IPipelineStep<T, R>
{
    private readonly SagaRegistry _sagaRegistry;

    public SaveSagaChangesStep(SagaRegistry sagaRegistry)
    {
        _sagaRegistry = sagaRegistry;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();
        await _sagaRegistry.SaveSagaChanges();
        return result;
    }
}
