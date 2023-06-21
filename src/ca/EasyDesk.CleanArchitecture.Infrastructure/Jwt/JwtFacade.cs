using NodaTime;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public sealed class JwtFacade
{
    private readonly IClock _clock;

    public JwtFacade(IClock clock)
    {
        _clock = clock;
    }

    public string Create(ClaimsIdentity claimsIdentity, Action<JwtGenerationBuilder> configure) =>
        Create(claimsIdentity, out _, configure);

    public string Create(ClaimsIdentity claimsIdentity, out JwtSecurityToken token, Action<JwtGenerationBuilder> configure)
    {
        var builder = new JwtGenerationBuilder(_clock.GetCurrentInstant());
        configure(builder);
        var descriptor = builder.Build();
        descriptor.Subject = claimsIdentity;

        var handler = new JwtSecurityTokenHandler();
        token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }

    public Option<ClaimsIdentity> Validate(string jwt, Action<JwtValidationBuilder> configure) =>
        Validate(jwt, out _, configure);

    public Option<ClaimsIdentity> Validate(string jwt, out JwtSecurityToken? token, Action<JwtValidationBuilder> configure)
    {
        var builder = new JwtValidationBuilder(_clock);
        configure(builder);
        var parameters = builder.Build();

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(jwt, parameters, out var genericToken);
            token = genericToken as JwtSecurityToken;
            return Some(principal.Identities.Single());
        }
        catch
        {
            token = null;
            return None;
        }
    }
}
