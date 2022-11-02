namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public interface IPipelineStep<in T, R>
{
    Task<Result<R>> Run(T request, NextPipelineStep<R> next);
}

public delegate Task<Result<R>> NextPipelineStep<R>();

////public interface IPipelineStep
////{
////    Option<PipelineStepRunner<T, R>> Get<T, R>();
////}

////public delegate Task<Result<R>> PipelineStepRunner<T, R>(T request, NextPipelineStep<R> next);

////public delegate Task<Result<R>> NextPipelineStep<R>();
