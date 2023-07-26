using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public abstract class AbstractAuthenticationHandler<T> : AuthenticationHandler<T>
    where T : AuthenticationSchemeOptions, new()
{
    protected AbstractAuthenticationHandler(IOptionsMonitor<T> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await Handle();
        return result
            .Map(principal => principal
                .IfFailure(error => Context.StoreAuthenticationError(error, Scheme.Name))
                .Match(
                    success: p => AuthenticateResult.Success(new AuthenticationTicket(p, Scheme.Name)),
                    failure: e => AuthenticateResult.Fail("Invalid authorization header")))
            .OrElseGet(AuthenticateResult.NoResult);
    }

    protected abstract Task<Option<Result<ClaimsPrincipal>>> Handle();
}
