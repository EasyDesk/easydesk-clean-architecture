using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public static class AuthenticationHandlerUtilities
{
    private const string AuthenticationErrorsKey = "__AuthenticationFailures";

    public static void StoreAuthenticationError(this HttpContext httpContext, Error error, string scheme)
    {
        httpContext.Items.Update(
            AuthenticationErrorsKey,
            x => ((IImmutableDictionary<string, Error>)x!).Add(scheme, error),
            () => Map((scheme, error)));
    }

    public static IImmutableDictionary<string, Error> RetrieveAuthenticationErrors(this HttpContext httpContext)
    {
        return httpContext.Items
            .GetOption(AuthenticationErrorsKey)
            .Map(x => (IImmutableDictionary<string, Error>)x!)
            .OrElseGet(() => Map<string, Error>());
    }
}
