using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
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
    public TokenReader TokenReader { get; set; } = _ => throw new InvalidOperationException("TokenReader not configured");
}

public abstract class TokenAuthenticationHandler<T> : AbstractAuthenticationHandler<T>
    where T : TokenAuthenticationOptions, new()
{
    public TokenAuthenticationHandler(IOptionsMonitor<T> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<Option<Result<ClaimsPrincipal>>> Handle()
    {
        return GetAuthenticateResult();
    }

    private Task<Option<Result<ClaimsPrincipal>>> GetAuthenticateResult()
    {
        return Options.TokenReader(Context).MapAsync(GetClaimsPrincipalFromToken);
    }

    protected abstract Task<Result<ClaimsPrincipal>> GetClaimsPrincipalFromToken(string token);
}
