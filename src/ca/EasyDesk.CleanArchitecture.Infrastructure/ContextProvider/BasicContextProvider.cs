using EasyDesk.CleanArchitecture.Application.ContextProvider;
using Microsoft.AspNetCore.Http;
using Rebus.Extensions;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

internal sealed class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipalParser<Agent> _agentParser;
    private readonly ITenantReader _tenantReader;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor, ClaimsPrincipalParser<Agent> agentParser, ITenantReader tenantReader)
    {
        _httpContextAccessor = httpContextAccessor;
        _agentParser = agentParser;
        _tenantReader = tenantReader;
    }

    public ContextInfo CurrentContext => MatchContext(
        httpContext: c => _agentParser(c.User).Match<ContextInfo>(
            some: agent => new ContextInfo.AuthenticatedRequest(agent),
            none: () => new ContextInfo.AnonymousRequest()),
        messageContext: _ => new ContextInfo.AsyncMessage(),
        other: () => new ContextInfo.Unknown());

    public CancellationToken CancellationToken => MatchContext(
        httpContext: c => c.RequestAborted,
        messageContext: c => c.GetCancellationToken(),
        other: () => default);

    public Option<string> TenantId => MatchContext(
        httpContext: _tenantReader.ReadFromHttpContext,
        messageContext: _tenantReader.ReadFromMessageContext,
        other: () => None);

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
