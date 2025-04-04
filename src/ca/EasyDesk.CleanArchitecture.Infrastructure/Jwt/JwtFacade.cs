﻿using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public record InvalidJwt(string Message) : ApplicationError
{
    public override string GetDetail() => "The given JWT is not valid";
}

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

    public Result<ClaimsIdentity> Validate(string jwt, Action<JwtValidationBuilder> configure) =>
        Validate(jwt, out _, configure);

    public Result<ClaimsIdentity> Validate(string jwt, out JwtSecurityToken? token, Action<JwtValidationBuilder> configure)
    {
        var builder = new JwtValidationBuilder(_clock);
        configure(builder);
        var parameters = builder.Build();

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(jwt, parameters, out var genericToken);
            token = genericToken as JwtSecurityToken;
            return principal.Identities.Single();
        }
        catch (Exception ex)
        {
            token = null;

            var message = ex switch
            {
                SecurityTokenDecryptionFailedException => "Decryption failed",
                SecurityTokenExpiredException => "Token expired",
                SecurityTokenInvalidAudienceException => "Invalid audience",
                SecurityTokenInvalidIssuerException => "Invalid issuer",
                SecurityTokenInvalidLifetimeException => "Invalid lifetime",
                SecurityTokenInvalidSignatureException => "Invalid signature",
                SecurityTokenInvalidSigningKeyException => "Invalid signing key",
                SecurityTokenNoExpirationException => "No expiration",
                SecurityTokenNotYetValidException => "Not yet valid",
                SecurityTokenReplayAddFailedException => "Replay add failed",
                SecurityTokenReplayDetectedException => "Replay detected",
                _ => "Token malformed",
            };

            return new InvalidJwt(message);
        }
    }
}
