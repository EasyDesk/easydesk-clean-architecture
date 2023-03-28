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

public class JwtValidationBuilder
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

    public JwtValidationBuilder WithSignatureValidation(SecurityKey key) =>
        NextStep(() =>
        {
            _parameters.ValidateIssuerSigningKey = true;
            _parameters.IssuerSigningKey = key;
            _hasSignatureValidation = true;
        });

    public JwtValidationBuilder WithoutLifetimeValidation() =>
        NextStep(() => _parameters.ValidateLifetime = false);

    public JwtValidationBuilder WithIssuerValidation(IEnumerable<string> validIssuers) =>
        NextStep(() =>
        {
            _parameters.ValidIssuers = validIssuers;
            _parameters.ValidateIssuer = true;
        });

    public JwtValidationBuilder WithIssuerValidation(string validIssuer) =>
        WithIssuerValidation(Items(validIssuer));

    public JwtValidationBuilder WithAudienceValidation(IEnumerable<string> validAudiences) =>
        NextStep(() =>
        {
            _parameters.ValidAudiences = validAudiences;
            _parameters.ValidateAudience = true;
        });

    public JwtValidationBuilder WithAudienceValidation(string validAudience) =>
        WithAudienceValidation(Items(validAudience));

    public JwtValidationBuilder WithClockSkew(Duration skew) =>
        NextStep(() => _parameters.ClockSkew = skew.ToTimeSpan());

    public JwtValidationBuilder WithDecryption(SecurityKey key) =>
        WithDecryption(Items(key));

    public JwtValidationBuilder WithDecryption(IEnumerable<SecurityKey> keys) =>
        NextStep(() => _parameters.TokenDecryptionKeys = keys);

    public JwtValidationBuilder NextStep(Action configureParameters)
    {
        configureParameters();
        return this;
    }
}
