using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Http;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public static class AuthenticationHandlerUtilities
{
    private const string AuthenticationErrorsKey = "__AuthenticationFailures";

    public static void StoreAuthenticationError(this HttpContext httpContext, Error error, string scheme)
    {
        httpContext.Items.Update(
            AuthenticationErrorsKey,
            x => ((IFixedMap<string, Error>)x!).Add(scheme, error),
            () => Map((scheme, error)));
    }

    public static IFixedMap<string, Error> RetrieveAuthenticationErrors(this HttpContext httpContext)
    {
        return httpContext.Items
            .GetOption(AuthenticationErrorsKey)
            .Map(x => (IFixedMap<string, Error>)x!)
            .OrElseGet(() => Map<string, Error>());
    }
}
