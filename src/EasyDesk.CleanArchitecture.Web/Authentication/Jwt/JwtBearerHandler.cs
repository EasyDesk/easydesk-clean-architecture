using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtBearerOptions : TokenAuthenticationOptions
{
    public JwtValidationConfiguration ConfigureValidation { get; private set; }

    public JwtBearerOptions ConfigureValidationParameters(JwtValidationConfiguration configure)
    {
        ConfigureValidation += configure;
        return this;
    }
}

public class JwtBearerHandler : TokenAuthenticationHandler<JwtBearerOptions>
{
    private readonly JwtFacade _jwtFacade;

    public JwtBearerHandler(
        IOptionsMonitor<JwtBearerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        JwtFacade jwtFacade)
        : base(options, logger, encoder, clock)
    {
        _jwtFacade = jwtFacade;
    }

    protected override Option<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
    {
        return _jwtFacade.Validate(token, Options.ConfigureValidation).Map(x => new ClaimsPrincipal(x));
    }
}
