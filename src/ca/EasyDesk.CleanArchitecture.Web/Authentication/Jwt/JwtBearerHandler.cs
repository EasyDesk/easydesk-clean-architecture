using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public sealed class JwtBearerOptions : TokenAuthenticationOptions
{
    public JwtValidationConfiguration Configuration { get; private set; } =
        JwtValidationConfiguration.FromKey(KeyUtils.RandomKey());

    public JwtBearerOptions ConfigureValidationParameters(JwtValidationConfiguration configure)
    {
        Configuration = configure;
        return this;
    }

    public JwtBearerOptions LoadParametersFromConfiguration(
        IConfiguration configuration, string sectionName = JwtConfigurationUtils.DefaultConfigurationSectionName)
    {
        return ConfigureValidationParameters(configuration.GetJwtValidationConfiguration(sectionName));
    }
}

internal class JwtBearerHandler : TokenAuthenticationHandler<JwtBearerOptions>
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
        return _jwtFacade
            .Validate(token, Options.Configuration.ConfigureBuilder)
            .Map(x => new ClaimsPrincipal(x));
    }
}
