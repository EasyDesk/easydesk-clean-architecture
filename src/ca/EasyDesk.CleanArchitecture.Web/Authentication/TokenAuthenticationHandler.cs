using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public delegate Option<string> TokenReader(HttpContext httpContext);

public abstract class TokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public TokenReader TokenReader { get; set; } = TokenReaders.Bearer();
}

public abstract class TokenAuthenticationHandler<T> : AuthenticationHandler<T>
    where T : TokenAuthenticationOptions, new()
{
    public TokenAuthenticationHandler(IOptionsMonitor<T> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(GetAuthenticateResult());
    }

    private AuthenticateResult GetAuthenticateResult()
    {
        var token = Options.TokenReader(Context);

        if (token.IsAbsent)
        {
            return AuthenticateResult.NoResult();
        }

        var claimsPrincipal = GetClaimsPrincipalFromToken(token.Value);
        return claimsPrincipal
            .Map(principal => new AuthenticationTicket(principal, Scheme.Name))
            .Match(
                some: AuthenticateResult.Success,
                none: () => AuthenticateResult.Fail("Invalid authorization header"));
    }

    protected abstract Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token);
}
