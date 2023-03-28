using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<Context> _context;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = new(GetContextType);
    }

    public Context Context => _context.Value;

    public Option<UserInfo> UserInfo => Context switch
    {
        AuthenticatedRequestContext(UserInfo info) => Some(info),
        _ => None
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
                .Select(c => new UserInfo(c.Value))
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
}
