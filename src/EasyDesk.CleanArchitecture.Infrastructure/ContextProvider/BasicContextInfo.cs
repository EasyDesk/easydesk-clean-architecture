﻿using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public class BasicContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BasicContextProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Context Context => GetContextType();

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