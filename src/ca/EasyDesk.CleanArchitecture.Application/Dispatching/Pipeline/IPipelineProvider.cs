namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

internal interface IPipelineProvider
{
    IEnumerable<IPipelineStep<T, R>> GetSteps<T, R>(IServiceProvider serviceProvider);
}
