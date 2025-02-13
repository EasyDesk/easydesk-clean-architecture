namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public interface IPipelineProvider
{
    IEnumerable<IPipelineStep<T, R>> GetSteps<T, R>(IServiceProvider serviceProvider);
}
