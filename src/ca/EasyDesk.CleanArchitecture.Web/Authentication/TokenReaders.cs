﻿using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public delegate Option<string> TokenReader(HttpContext httpContext);

public static class TokenReaders
{
    public static TokenReader Combine(params TokenReader[] readers) => httpContext => readers
        .SelectMany(r => r(httpContext))
        .FirstOption();

    public static TokenReader FromAuthorizationHeader(string scheme) => httpContext => httpContext
        .Request
        .Headers
        .GetOption(HeaderNames.Authorization)
        .MapToString()
        .FlatMap(ParseHeader)
        .Filter(x => x.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
        .FlatMap(x => x.Parameter.AsOption());

    public static TokenReader Bearer() => FromAuthorizationHeader(JwtBearerDefaults.AuthenticationScheme);

    public static TokenReader FromQueryParameter(string queryParameter) =>
        httpContext => GetQueryParameterAsOption(queryParameter, httpContext).MapToString();

    private static Option<StringValues> GetQueryParameterAsOption(string queryParameter, HttpContext httpContext)
    {
        return TryOption<string, StringValues>(httpContext.Request.Query.TryGetValue, queryParameter);
    }

    private static Option<AuthenticationHeaderValue> ParseHeader(string rawHeader)
    {
        return TryOption<string, AuthenticationHeaderValue>(AuthenticationHeaderValue.TryParse, rawHeader);
    }
}
