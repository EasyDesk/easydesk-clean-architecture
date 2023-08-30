using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class ContextResetStep<T, R> : IPipelineStep<T, R>
{
    private readonly IContextResetter _contextResetter;

    public ContextResetStep(IContextResetter contextResetter)
    {
        _contextResetter = contextResetter;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();
        _contextResetter.ResetContext();
        return result;
    }
}
