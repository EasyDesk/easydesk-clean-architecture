using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Extensions;
using Rebus.Pipeline;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class ContextProviderOptions
{
    private readonly Dictionary<string, string> _claimToAttributes = new();

    public ContextProviderOptions AddAttributeFromClaim(string claim, string attribute)
    {
        _claimToAttributes.Add(claim, attribute);
        return this;
    }

    internal Option<string> ClaimToAttribute(string claim) => _claimToAttributes.GetOption(claim);
}

internal sealed class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ContextProviderOptions _options;
    private readonly Lazy<ContextInfo> _context;
    private readonly Lazy<CancellationToken> _cancellationToken;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor, ContextProviderOptions options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _context = new(GetContextType);
        _cancellationToken = new(GetCancellationToken);
    }

    public ContextInfo CurrentContext => _context.Value;

    public CancellationToken CancellationToken => _cancellationToken.Value;

    private ContextInfo GetContextType()
    {
        return MatchContext(
            httpContext: c => c.User
                .Identities
                .Where(i => i.IsAuthenticated)
                .SelectMany(i => i.FindFirst(ClaimTypes.NameIdentifier).AsOption())
                .Select(c => c.Value)
                .Select(id => UserInfo.Create(UserId.New(id), GetUserAttributes(c.User)))
                .FirstOption()
                .Match<ContextInfo>(
                    some: userInfo => new ContextInfo.AuthenticatedRequest(userInfo),
                    none: () => new ContextInfo.AnonymousRequest()),
            messageContext: _ => new ContextInfo.AsyncMessage(),
            other: () => new ContextInfo.Unknown());
    }

    private AttributeCollection GetUserAttributes(ClaimsPrincipal claimsPrincipal)
    {
        var pairs = claimsPrincipal
            .Identities
            .Where(i => i.IsAuthenticated)
            .SelectMany(i => i.Claims)
            .SelectMany(c => _options.ClaimToAttribute(c.Type).Map(a => (Key: a, c.Value)));

        return AttributeCollection.FromFlatKeyValuePairs(pairs);
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
