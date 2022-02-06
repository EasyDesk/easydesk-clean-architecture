using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using Microsoft.IdentityModel.Tokens;
using System;
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

    public string Create(IEnumerable<Claim> claims, out JwtSecurityToken token, JwtTokenConfiguration configure)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = _timestampProvider.Now.AsDateTime
        };
        configure(new JwtTokenBuilder<JwtTokenBuilderSteps.Initial>(descriptor));

        var handler = new JwtSecurityTokenHandler();
        token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }

    public string Create(IEnumerable<Claim> claims, JwtTokenConfiguration configure) => Create(claims, out _, configure);

    private TokenValidationParameters CreateDefaultValidationParameters()
    {
        return new TokenValidationParameters
        {
            LifetimeValidator = ValidateLifetime,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero
        };
    }

    private bool ValidateLifetime(DateTime? nbf, DateTime? exp, SecurityToken token, TokenValidationParameters validationParameters)
    {
        if (!validationParameters.ValidateLifetime)
        {
            return true;
        }

        var now = _timestampProvider.Now.AsDateTime;
        if (nbf is not null && now < nbf - validationParameters.ClockSkew)
        {
            return false;
        }
        if (exp is not null && now > exp + validationParameters.ClockSkew)
        {
            return false;
        }
        return true;
    }

    public Option<ClaimsPrincipal> Validate(string jwt, out JwtSecurityToken token, JwtValidationConfiguration configure)
    {
        var parameters = CreateDefaultValidationParameters();
        configure(new JwtValidationBuilder<JwtValidationSteps.Initial>(parameters));

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(jwt, parameters, out var genericToken);
            token = genericToken as JwtSecurityToken;
            return principal;
        }
        catch
        {
            token = null;
            return None;
        }
    }

    public Option<ClaimsPrincipal> Validate(string jwt, JwtValidationConfiguration configure) =>
        Validate(jwt, out _, configure);
}
