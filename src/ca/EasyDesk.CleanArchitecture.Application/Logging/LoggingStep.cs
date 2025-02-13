using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Results;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Application.Logging;

public class LoggingStep<T, R> : IPipelineStep<T, R>
{
    private readonly ILogger<LoggingStep<T, R>> _logger;
    private readonly LoggingConfiguration _loggingConfiguration;

    public LoggingStep(ILogger<LoggingStep<T, R>> logger, LoggingConfiguration loggingConfiguration)
    {
        _logger = logger;
        _loggingConfiguration = loggingConfiguration;
    }

    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var id = Guid.NewGuid();

        _logger.LogInformation("Request({id}) - Handling request of type {requestType}.", id, typeof(T).Name);

        if (_loggingConfiguration.RequestLoggingEnabled)
        {
            _logger.LogInformation("Request({id}) - Request content: {requestContent}", id, request);
        }

        var result = await next();

        if (_loggingConfiguration.ResultLoggingEnabled)
        {
            _logger.LogInformation("Request({id}) - Result content: {resultContent}", id, result);
        }

        return result;
    }
}
