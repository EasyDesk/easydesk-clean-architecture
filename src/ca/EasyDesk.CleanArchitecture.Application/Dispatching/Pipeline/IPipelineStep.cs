using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public interface IPipelineStep<in T, R>
{
    Task<Result<R>> Run(T request, NextPipelineStep<R> next);

    bool IsForEachHandler { get; }
}

public delegate Task<Result<R>> NextPipelineStep<R>();

public static class PipelineStepExtensions
{
    public static async Task<Result<R>> Run<T, R>(this IEnumerable<IPipelineStep<T, R>> steps, T request, AsyncFunc<T, Result<R>> action)
    {
        return await steps.FoldRight<IPipelineStep<T, R>, NextPipelineStep<R>>(
            () => action(request),
            (step, next) => () => step.Run(request, next))();
    }
}
