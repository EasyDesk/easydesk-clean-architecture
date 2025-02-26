using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.Logging;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtLogger
{
    private readonly AuthenticationModuleOptions _authModuleOptions;
    private readonly JwtFacade _jwtFacade;
    private readonly ILogger<JwtLogger> _logger;

    public JwtLogger(
        AuthenticationModuleOptions authModuleOptions,
        JwtFacade jwtFacade,
        ILogger<JwtLogger> logger)
    {
        _authModuleOptions = authModuleOptions;
        _jwtFacade = jwtFacade;
        _logger = logger;
    }

    public void LogForgedJwt(Agent agent, string? schemeName = null)
    {
        var jwtBearerProvider = FindJwtBearerProvider(schemeName);
        var jwtConfiguration = jwtBearerProvider.Options.Value.Configuration.ToJwtGenerationConfiguration();
        var jwt = _jwtFacade.Create(agent.ToClaimsIdentity(), jwtConfiguration.ConfigureBuilder);
        _logger.LogWarning("Forged JWT: {jwt}", jwt);
    }

    private JwtBearerProvider FindJwtBearerProvider(string? schemeName = null)
    {
        if (schemeName is null)
        {
            return _authModuleOptions.Schemes.Values
                .SelectMany(s => (s as JwtBearerProvider).AsOption())
                .FirstOption()
                .OrElseThrow(() => new InvalidOperationException(
                    $"Missing a {nameof(JwtBearerProvider)} in the authentication configuration"));
        }

        if (_authModuleOptions.Schemes[schemeName] is not JwtBearerProvider jwtProvider)
        {
            throw new InvalidOperationException($"Scheme {schemeName} is not of type {nameof(JwtBearerProvider)}.");
        }

        return jwtProvider;
    }
}
