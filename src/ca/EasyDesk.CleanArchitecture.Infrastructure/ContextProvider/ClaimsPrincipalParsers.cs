﻿using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public static class ClaimsPrincipalParsers
{
    public static ClaimsPrincipalParser<R> Map<T, R>(this ClaimsPrincipalParser<T> parser, Func<T, R> mapper) =>
        cp => parser(cp).Map(mapper);

    public static ClaimsPrincipalParser<string> ForClaim(string claimName) =>
        cp => cp.FindFirstValue(claimName).AsOption();

    public static ClaimsPrincipalParser<Agent> ForAgent(Action<AgentParserBuilder> configure)
    {
        var builder = new AgentParserBuilder();
        configure(builder);
        return builder.Build();
    }

    public static ClaimsPrincipalParser<Agent> DefaultAgentParser() =>
        cp => cp.Identities.Where(x => x.IsAuthenticated).FirstOption().Map(x => x.ToAgent());
}
