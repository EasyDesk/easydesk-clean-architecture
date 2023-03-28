using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;

public class JwtHttpAuthentication : ITestHttpAuthentication
{
    private readonly JwtFacade _jwtFacade;
    private readonly JwtGenerationConfiguration _jwtGenerationConfiguration;

    public JwtHttpAuthentication(JwtFacade jwtFacade, JwtGenerationConfiguration jwtGenerationConfiguration)
    {
        _jwtFacade = jwtFacade;
        _jwtGenerationConfiguration = jwtGenerationConfiguration;
    }

    public ImmutableHttpRequestMessage ConfigureAuthentication(ImmutableHttpRequestMessage message, IEnumerable<Claim> identity) =>
        message with { Headers = message.Headers.Replace(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {ForgeJwt(identity)}") };

    public ImmutableHttpRequestMessage RemoveAuthentication(ImmutableHttpRequestMessage message) =>
        message with { Headers = message.Headers.Remove(HeaderNames.Authorization) };

    private string ForgeJwt(IEnumerable<Claim> identity) =>
        _jwtFacade.Create(identity, _jwtGenerationConfiguration.ConfigureBuilder);
}
