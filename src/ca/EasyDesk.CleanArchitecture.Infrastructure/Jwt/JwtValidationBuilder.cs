using EasyDesk.Commons.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.EnumerableUtils;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public record JwtValidationConfiguration(
    SecurityKey ValidationKey,
    IImmutableSet<string> Issuers,
    IImmutableSet<string> Audiences,
    IEnumerable<SecurityKey> DecryptionKeys,
    Option<Duration> ClockSkew = default,
    bool ValidateLifetime = true)
{
    public static JwtValidationConfiguration FromKey(SecurityKey validationKey) => new(
        ValidationKey: validationKey,
        Issuers: Set<string>(),
        Audiences: Set<string>(),
        DecryptionKeys: Enumerable.Empty<SecurityKey>());

    public void ConfigureBuilder(JwtValidationBuilder builder)
    {
        builder
            .WithSignatureValidation(ValidationKey);
        if (Issuers.Any())
        {
            builder.WithIssuerValidation(Issuers);
        }
        if (Audiences.Any())
        {
            builder.WithAudienceValidation(Audiences);
        }
        if (DecryptionKeys.Any())
        {
            builder.WithDecryption(DecryptionKeys);
        }
        if (!ValidateLifetime)
        {
            builder.WithoutLifetimeValidation();
        }
        ClockSkew.IfPresent(skew => builder.WithClockSkew(skew));
    }
}

public sealed class JwtValidationBuilder
{
    private bool _wasBuilt = false;
    private bool _hasSignatureValidation = false;
    private readonly TokenValidationParameters _parameters;
    private readonly IClock _clock;

    public JwtValidationBuilder(IClock clock)
    {
        _clock = clock;
        _parameters = new TokenValidationParameters
        {
            LifetimeValidator = ValidateLifetime,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero,
        };
    }

    private bool ValidateLifetime(DateTime? nbf, DateTime? exp, SecurityToken token, TokenValidationParameters validationParameters)
    {
        if (!validationParameters.ValidateLifetime)
        {
            return true;
        }

        var now = _clock.GetCurrentInstant().ToDateTimeUtc();
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

    public TokenValidationParameters Build()
    {
        if (_wasBuilt)
        {
            throw new InvalidOperationException("Cannot call Build() multiple times");
        }
        if (!_hasSignatureValidation)
        {
            throw new InvalidOperationException("Cannot validate token without signature validation");
        }

        _wasBuilt = true;
        return _parameters;
    }

    public JwtValidationBuilder WithSignatureValidation(SecurityKey key, params SecurityKey[] keys) =>
        WithSignatureValidation(keys.Prepend(key));

    public JwtValidationBuilder WithSignatureValidation(IEnumerable<SecurityKey> keys)
    {
        if (keys.IsEmpty())
        {
            throw new ArgumentException($"Signature validation must have at least one key");
        }

        _parameters.ValidateIssuerSigningKey = true;
        _parameters.IssuerSigningKeys = keys;
        _hasSignatureValidation = true;
        return this;
    }

    public JwtValidationBuilder WithoutLifetimeValidation()
    {
        _parameters.ValidateLifetime = false;
        return this;
    }

    public JwtValidationBuilder WithIssuerValidation(IEnumerable<string> validIssuers)
    {
        _parameters.ValidIssuers = validIssuers;
        _parameters.ValidateIssuer = true;
        return this;
    }

    public JwtValidationBuilder WithIssuerValidation(string validIssuer) =>
        WithIssuerValidation(Items(validIssuer));

    public JwtValidationBuilder WithAudienceValidation(IEnumerable<string> validAudiences)
    {
        _parameters.ValidAudiences = validAudiences;
        _parameters.ValidateAudience = true;
        return this;
    }

    public JwtValidationBuilder WithAudienceValidation(string validAudience) =>
        WithAudienceValidation(Items(validAudience));

    public JwtValidationBuilder WithClockSkew(Duration skew)
    {
        _parameters.ClockSkew = skew.ToTimeSpan();
        return this;
    }

    public JwtValidationBuilder WithDecryption(SecurityKey key) =>
        WithDecryption(Items(key));

    public JwtValidationBuilder WithDecryption(IEnumerable<SecurityKey> keys)
    {
        _parameters.TokenDecryptionKeys = keys;
        return this;
    }
}
