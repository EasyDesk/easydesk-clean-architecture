using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Http;
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
    private readonly Lazy<Context> _context;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor, ContextProviderOptions options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _context = new(GetContextType);
    }

    public Context Context => _context.Value;

    public Option<UserInfo> User => Context switch
    {
        AuthenticatedRequestContext(UserInfo info) => Some(info),
        _ => None,
    };

    private Context GetContextType()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            return httpContext.User
                .Identities
                .Where(i => i.IsAuthenticated)
                .SelectMany(i => i.FindFirst(ClaimTypes.NameIdentifier).AsOption())
                .Select(c => c.Value)
                .Select(id => UserInfo.Create(UserId.New(id), GetUserAttributes(httpContext.User)))
                .FirstOption()
                .Match<Context>(
                    some: userInfo => new AuthenticatedRequestContext(userInfo),
                    none: () => new AnonymousRequestContext());
        }

        if (MessageContext.Current is not null)
        {
            return new AsyncMessageContext();
        }

        return new NoContext();
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
}
