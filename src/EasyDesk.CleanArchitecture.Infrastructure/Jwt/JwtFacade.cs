using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public class JwtFacade
{
    private readonly ITimestampProvider _timestampProvider;

    public JwtFacade(ITimestampProvider timestampProvider)
    {
        _timestampProvider = timestampProvider;
    }

    public string Create(IEnumerable<Claim> claims, JwtTokenConfiguration configure) =>
        Create(claims, out _, configure);

    public string Create(IEnumerable<Claim> claims, out JwtSecurityToken token, JwtTokenConfiguration configure)
    {
        var builder = new JwtTokenBuilder(_timestampProvider.Now).WithClaims(claims);
        configure(builder);
        var descriptor = builder.Build();

        var handler = new JwtSecurityTokenHandler();
        token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }

    public Option<ClaimsPrincipal> Validate(string jwt, JwtValidationConfiguration configure) =>
        Validate(jwt, out _, configure);

    public Option<ClaimsPrincipal> Validate(string jwt, out JwtSecurityToken token, JwtValidationConfiguration configure)
    {
        var builder = new JwtValidationBuilder(_timestampProvider);
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
