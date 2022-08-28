namespace EasyDesk.CleanArchitecture.Application.Cqrs.Pipeline;

public interface IPipelineStep<TRequest, TResult>
    where TRequest : ICqrsRequest<TResult>
{
    Task<Result<TResult>> Run(TRequest request, NextPipelineStep<TResult> next);
}

public delegate Task<Result<TResult>> NextPipelineStep<TResult>();
