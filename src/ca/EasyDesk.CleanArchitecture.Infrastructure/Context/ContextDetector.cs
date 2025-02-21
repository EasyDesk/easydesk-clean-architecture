using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Context;

public sealed class ContextDetector
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextDetector(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public T MatchContext<T>(Func<HttpContext, T> httpContext, Func<IMessageContext, T> messageContext, Func<T> other)
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

        return other();
    }
}
