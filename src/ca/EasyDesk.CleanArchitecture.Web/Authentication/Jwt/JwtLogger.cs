using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtLogger
{
    private readonly AuthenticationModuleOptions _authModuleOptions;
    private readonly IOptionsMonitor<JwtBearerOptions> _jwtBearerOptions;
    private readonly JwtFacade _jwtFacade;
    private readonly ILogger<JwtLogger> _logger;

    public JwtLogger(
        AuthenticationModuleOptions authModuleOptions,
        IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
        JwtFacade jwtFacade,
        ILogger<JwtLogger> logger)
    {
        _authModuleOptions = authModuleOptions;
        _jwtBearerOptions = jwtBearerOptions;
        _jwtFacade = jwtFacade;
        _logger = logger;
    }

    public void LogForgedJwt(Agent agent, string? schemeName = null)
    {
        var scheme = schemeName ?? _authModuleOptions.Schemes
            .Where(s => s.Value is JwtAuthenticationProvider)
            .Select(s => s.Key)
            .FirstOption()
            .OrElseThrow(() => new InvalidOperationException(
                $"Missing a {nameof(JwtAuthenticationProvider)} in the authentication configuration"));

        var jwtConfiguration = _jwtBearerOptions.Get(schemeName).Configuration.ToJwtGenerationConfiguration();
        var jwt = _jwtFacade.Create(agent.ToClaimsIdentity(), jwtConfiguration.ConfigureBuilder);
        _logger.LogWarning("Forged JWT: {jwt}", jwt);
    }
}
