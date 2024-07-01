using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

internal class DefaultPipeline : IPipeline
{
    private readonly IPipelineProvider _pipelineProvider;
    private readonly IServiceProvider _serviceProvider;

    public DefaultPipeline(IPipelineProvider pipelineProvider, IServiceProvider serviceProvider)
    {
        _pipelineProvider = pipelineProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<Result<R>> Run<T, R>(T request, AsyncFunc<T, Result<R>> action)
    {
        var steps = _pipelineProvider.GetSteps<T, R>(_serviceProvider);
        return await steps.FoldRight<IPipelineStep<T, R>, NextPipelineStep<R>>(
            () => action(request),
            (step, next) => () => step.Run(request, next))();
    }
}
