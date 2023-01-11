namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

internal interface IPipeline
{
    IEnumerable<IPipelineStep<T, R>> GetSteps<T, R>(IServiceProvider serviceProvider);
}
