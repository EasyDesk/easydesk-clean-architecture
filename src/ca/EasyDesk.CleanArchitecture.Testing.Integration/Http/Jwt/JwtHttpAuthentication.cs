using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;

public sealed class JwtHttpAuthentication : ITestHttpAuthentication
{
    private readonly JwtFacade _jwtFacade;
    private readonly JwtGenerationConfiguration _jwtGenerationConfiguration;

    public JwtHttpAuthentication(JwtFacade jwtFacade, JwtGenerationConfiguration jwtGenerationConfiguration)
    {
        _jwtFacade = jwtFacade;
        _jwtGenerationConfiguration = jwtGenerationConfiguration;
    }

    public ImmutableHttpRequestMessage ConfigureAuthentication(ImmutableHttpRequestMessage message, Agent agent) =>
        message with
        {
            Headers = message.Headers.Replace(
                HeaderNames.Authorization,
                $"{JwtBearerDefaults.AuthenticationScheme} {ForgeJwt(agent)}"),
        };

    public ImmutableHttpRequestMessage RemoveAuthentication(ImmutableHttpRequestMessage message) =>
        message with { Headers = message.Headers.Remove(HeaderNames.Authorization) };

    private string ForgeJwt(Agent agent) =>
        _jwtFacade.Create(agent.ToClaimsIdentity(), _jwtGenerationConfiguration.ConfigureBuilder);
}
