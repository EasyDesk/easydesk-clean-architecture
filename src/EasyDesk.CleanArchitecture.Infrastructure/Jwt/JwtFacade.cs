using NodaTime;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public class JwtFacade
{
    private readonly IClock _clock;

    public JwtFacade(IClock clock)
    {
        _clock = clock;
    }

    public string Create(IEnumerable<Claim> claims, Action<JwtGenerationBuilder> configure) =>
        Create(claims, out _, configure);

    public string Create(IEnumerable<Claim> claims, out JwtSecurityToken token, Action<JwtGenerationBuilder> configure)
    {
        var builder = new JwtGenerationBuilder(_clock.GetCurrentInstant()).WithClaims(claims);
        configure(builder);
        var descriptor = builder.Build();

        var handler = new JwtSecurityTokenHandler();
        token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }

    public Option<ClaimsPrincipal> Validate(string jwt, Action<JwtValidationBuilder> configure) =>
        Validate(jwt, out _, configure);

    public Option<ClaimsPrincipal> Validate(string jwt, out JwtSecurityToken? token, Action<JwtValidationBuilder> configure)
    {
        var builder = new JwtValidationBuilder(_clock);
        configure(builder);
        var parameters = builder.Build();

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(jwt, parameters, out var genericToken);
            token = genericToken as JwtSecurityToken;
            return Some(principal);
        }
        catch
        {
            token = null;
            return None;
        }
    }
}
