using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public record JwtSettings(
    Duration Lifetime,
    SecurityKey Key);

public class JwtService
{
    private readonly JwtSecurityTokenHandler _handler;
    private readonly ITimestampProvider _timestampProvider;

    public JwtService(ITimestampProvider timestampProvider)
    {
        _handler = new JwtSecurityTokenHandler();
        _handler.OutboundClaimTypeMap.Clear();
        _handler.InboundClaimTypeMap.Clear();
        _timestampProvider = timestampProvider;
    }

    public static TimeOffset ClockSkew { get; } = TimeOffset.FromMinutes(1);

    public Option<ClaimsIdentity> Validate(string jwt, JwtSettings settings, out JwtSecurityToken token, bool validateLifetime = true)
    {
        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = ClockSkew.AsTimeSpan,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = settings.Key,
            ValidateLifetime = validateLifetime
        };
        return ValidateUsingParameters(jwt, validationParameters, out token);
    }

    public Option<ClaimsIdentity> Validate(string jwt, TokenValidationParameters validationParameters, out JwtSecurityToken token)
    {
        validationParameters = validationParameters.Clone();
        return ValidateUsingParameters(jwt, validationParameters, out token);
    }

    private Option<ClaimsIdentity> ValidateUsingParameters(
        string jwt,
        TokenValidationParameters validationParameters,
        out JwtSecurityToken token)
    {
        try
        {
            validationParameters.LifetimeValidator = ValidateLifetime;

            var principal = _handler.ValidateToken(jwt, validationParameters, out var genericToken);
            token = genericToken as JwtSecurityToken;

            return principal.Identities.FirstOption();
        }
        catch
        {
            token = null;
            return None;
        }
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

    public string CreateToken(ClaimsIdentity identity, JwtSettings settings, out JwtSecurityToken token)
    {
        var now = _timestampProvider.Now.AsDateTime;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = identity,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + settings.Lifetime.AsTimeSpan,
            SigningCredentials = CreateSigningCredentials(settings.Key)
        };
        token = _handler.CreateToken(descriptor) as JwtSecurityToken;
        return _handler.WriteToken(token);
    }

    private SigningCredentials CreateSigningCredentials(SecurityKey key)
    {
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
    }
}
