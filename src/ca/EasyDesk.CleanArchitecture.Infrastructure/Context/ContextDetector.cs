using EasyDesk.CleanArchitecture.Application.CommandLine;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Context;

public sealed class ContextDetector
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CliContextAccessor _cliContextAccessor;

    public ContextDetector(IHttpContextAccessor httpContextAccessor, CliContextAccessor cliContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _cliContextAccessor = cliContextAccessor;
    }

    public T MatchContext<T>(Func<HttpContext, T> httpContext, Func<IMessageContext, T> messageContext, Func<CliContext, T> cliContext, Func<T> other)
    {
        var httpContextInstance = _httpContextAccessor.HttpContext;
        if (httpContextInstance is not null)
        {
            return httpContext(httpContextInstance);
        }

        var messageContextInstance = MessageContext.Current;
        if (messageContextInstance is not null)
        {
            return messageContext(messageContextInstance);
        }

        var cliContextInstance = _cliContextAccessor.CliContext;
        if (cliContextInstance.IsPresent)
        {
            return cliContext(cliContextInstance.Value);
        }

        return other();
    }
}
