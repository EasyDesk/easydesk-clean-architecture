using EasyDesk.CleanArchitecture.Application.ContextProvider;
using Microsoft.AspNetCore.Http;
using Rebus.Extensions;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

internal sealed class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipalParser<Agent> _agentParser;
    private readonly Lazy<ContextInfo> _context;
    private readonly Lazy<CancellationToken> _cancellationToken;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor, ClaimsPrincipalParser<Agent> agentParser)
    {
        _httpContextAccessor = httpContextAccessor;
        _agentParser = agentParser;
        _context = new(GetContextType);
        _cancellationToken = new(GetCancellationToken);
    }

    public ContextInfo CurrentContext => _context.Value;

    public CancellationToken CancellationToken => _cancellationToken.Value;

    private ContextInfo GetContextType()
    {
        return MatchContext(
            httpContext: c => _agentParser(c.User).Match<ContextInfo>(
                some: agent => new ContextInfo.AuthenticatedRequest(agent),
                none: () => new ContextInfo.AnonymousRequest()),
            messageContext: _ => new ContextInfo.AsyncMessage(),
            other: () => new ContextInfo.Unknown());
    }

    private CancellationToken GetCancellationToken()
    {
        return MatchContext(
            httpContext: c => c.RequestAborted,
            messageContext: c => c.GetCancellationToken(),
            other: () => default);
    }

    private T MatchContext<T>(Func<HttpContext, T> httpContext, Func<IMessageContext, T> messageContext, Func<T> other)
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
