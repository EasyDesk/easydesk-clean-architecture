using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;

public class JwtHttpAuthentication : ITestHttpAuthentication
{
    private readonly JwtFacade _jwtFacade;
    private readonly JwtTokenConfiguration _jwtTokenConfiguration;

    public JwtHttpAuthentication(JwtFacade jwtFacade, JwtTokenConfiguration jwtTokenConfiguration)
    {
        _jwtFacade = jwtFacade;
        _jwtTokenConfiguration = jwtTokenConfiguration;
    }

    public void ConfigureAuthentication(HttpRequestBuilder builder, IEnumerable<Claim> identity) =>
        builder.Headers(h => h.Replace(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {ForgeJwt(identity)}"));

    public void RemoveAuthentication(HttpRequestBuilder builder) =>
        builder.Headers(h => h.Remove(HeaderNames.Authorization));

    private string ForgeJwt(IEnumerable<Claim> identity) =>
        _jwtFacade.Create(identity, _jwtTokenConfiguration.ConfigureBuilder);
}
