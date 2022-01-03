using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public abstract class BaseAuthenticationHandler<T> : AuthenticationHandler<T>
    where T : AuthenticationSchemeOptions, new()
{
    private readonly string _scheme;

    public BaseAuthenticationHandler(
        IOptionsMonitor<T> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        string scheme)
        : base(options, logger, encoder, clock)
    {
        _scheme = scheme;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(GetAuthenticateResult());
    }

    private AuthenticateResult GetAuthenticateResult()
    {
        var token = Request.Headers
            .GetOption("Authorization")
            .FlatMap(x => ParseHeader(x))
            .Filter(x => x.Scheme.Equals(_scheme, StringComparison.OrdinalIgnoreCase))
            .Map(x => x.Parameter);

        if (token.IsAbsent)
        {
            return AuthenticateResult.NoResult();
        }

        var claimsPrincipal = GetClaimsPrincipalFromToken(token.Value);
        return claimsPrincipal
            .Map(x => new AuthenticationTicket(x, Scheme.Name))
            .Match(
                some: AuthenticateResult.Success,
                none: () => AuthenticateResult.Fail("Invalid authorization header"));
    }

    private Option<AuthenticationHeaderValue> ParseHeader(string rawHeader)
    {
        return OptionImports.FromTryConstruct<string, AuthenticationHeaderValue>(rawHeader, AuthenticationHeaderValue.TryParse);
    }

    protected abstract Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token);
}
