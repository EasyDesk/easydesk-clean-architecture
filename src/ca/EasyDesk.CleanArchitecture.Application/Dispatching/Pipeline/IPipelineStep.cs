namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public interface IPipelineStep<in T, R>
    where R : notnull
{
    Task<Result<R>> Run(T request, NextPipelineStep<R> next);
}

public delegate Task<Result<R>> NextPipelineStep<R>() where R : notnull;
