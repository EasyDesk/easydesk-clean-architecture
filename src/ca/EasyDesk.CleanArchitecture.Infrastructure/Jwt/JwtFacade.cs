using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using Microsoft.IdentityModel.Tokens;
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
                SecurityTokenDecryptionFailedException => "DecryptionFailed",
                SecurityTokenExpiredException => "TokenExpired",
                SecurityTokenInvalidAudienceException => "InvalidAudience",
                SecurityTokenInvalidIssuerException => "InvalidIssuer",
                SecurityTokenInvalidLifetimeException => "InvalidLifetime",
                SecurityTokenInvalidSignatureException => "InvalidSignature",
                SecurityTokenInvalidSigningKeyException => "InvalidSigningKey",
                SecurityTokenNoExpirationException => "NoExpiration",
                SecurityTokenNotYetValidException => "NotYetValid",
                SecurityTokenReplayAddFailedException => "ReplayAddFailed",
                SecurityTokenReplayDetectedException => "ReplayDetected",
                _ => "TokenMalformed"
            };

            return Errors.Generic("Failed to validate JWT. Failure code: {reason}", message);
        }
    }
}
