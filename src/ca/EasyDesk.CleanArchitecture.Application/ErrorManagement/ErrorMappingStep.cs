using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public class ErrorMappingStep<T, R> : IPipelineStep<T, R>
{
    private readonly GlobalErrorMapper _globalErrorMapper;

    public ErrorMappingStep(GlobalErrorMapper globalErrorMapper)
    {
        _globalErrorMapper = globalErrorMapper;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();
        var version = typeof(T).GetApiVersionFromNamespace();
        return result.MapError(e => _globalErrorMapper.MapError(e, version));
    }
}
