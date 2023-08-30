using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.AspNetCore.Http;
using Rebus.Extensions;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public delegate Option<string> HttpRequestTenantReader(HttpContext httpContext, Option<Agent> agent);

public delegate Option<string> AsyncMessageTenantReader(IMessageContext messageContext);

internal sealed class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ClaimsPrincipalParser<Agent> _agentParser;
    private readonly HttpRequestTenantReader _httpRequestTenantReader;
    private readonly AsyncMessageTenantReader _asyncMessageTenantReader;

    public BasicContextProvider(
        IHttpContextAccessor httpContextAccessor,
        ClaimsPrincipalParser<Agent> agentParser,
        HttpRequestTenantReader httpRequestTenantReader,
        AsyncMessageTenantReader asyncMessageTenantReader)
    {
        _httpContextAccessor = httpContextAccessor;
        _agentParser = agentParser;
        _httpRequestTenantReader = httpRequestTenantReader;
        _asyncMessageTenantReader = asyncMessageTenantReader;
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
        httpContext: c => _httpRequestTenantReader(c, ParseAgent(c)),
        messageContext: c => _asyncMessageTenantReader(c),
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

    private Option<Agent> ParseAgent(HttpContext context) =>
        (Option<Agent>)context.Items.GetOrAdd(typeof(Agent), () => _agentParser(context.User))!;
}
