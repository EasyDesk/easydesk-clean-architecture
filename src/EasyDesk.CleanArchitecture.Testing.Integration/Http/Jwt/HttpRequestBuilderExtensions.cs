using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;

public static class HttpRequestBuilderExtensions
{
    public static HttpRequestBuilder AuthenticateAs(this HttpRequestBuilder builder, string userId) =>
        builder.Authenticate(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });
}
