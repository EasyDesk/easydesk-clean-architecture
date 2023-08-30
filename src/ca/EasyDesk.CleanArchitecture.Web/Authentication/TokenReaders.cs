using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

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

    public static TokenReader Bearer() => FromAuthorizationHeader("Bearer");

    private static Option<AuthenticationHeaderValue> ParseHeader(string rawHeader)
    {
        return TryOption<string, AuthenticationHeaderValue>(AuthenticationHeaderValue.TryParse, rawHeader);
    }
}
