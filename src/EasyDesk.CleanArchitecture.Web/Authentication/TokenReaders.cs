using EasyDesk.Tools.Collections;
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
        .GetOption("Authorization")
        .FlatMap(x => ParseHeader(x))
        .Filter(x => x.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
        .Map(x => x.Parameter);

    public static TokenReader Bearer() => FromAuthorizationHeader("Bearer");

    private static Option<AuthenticationHeaderValue> ParseHeader(string rawHeader)
    {
        return TryOption<string, AuthenticationHeaderValue>(AuthenticationHeaderValue.TryParse, rawHeader);
    }
}
