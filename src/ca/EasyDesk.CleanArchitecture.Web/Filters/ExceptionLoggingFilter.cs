using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public class ExceptionLoggingFilter : IExceptionFilter
{
    private readonly ILogger _logger;

    public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "An unhandled exception of type {exceptionType} was thrown", context.Exception.GetType().Name);
    }
}
