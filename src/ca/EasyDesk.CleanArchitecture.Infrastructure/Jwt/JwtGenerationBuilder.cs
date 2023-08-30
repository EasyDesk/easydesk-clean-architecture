using EasyDesk.Commons.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public record JwtGenerationConfiguration(
    SigningCredentials SigningCredentials,
    Duration Lifetime,
    Option<string> Issuer = default,
    Option<string> Audience = default,
    Option<string> CompressionAlgorithm = default,
    Option<EncryptingCredentials> EncryptingCredentials = default)
{
    public void ConfigureBuilder(JwtGenerationBuilder builder)
    {
        builder
            .WithSigningCredentials(SigningCredentials)
            .WithLifetime(Lifetime);
        Issuer.IfPresent(issuer => builder.WithIssuer(issuer));
        Audience.IfPresent(audience => builder.WithAudience(audience));
        CompressionAlgorithm.IfPresent(algorithm => builder.WithCompressionAlgorithm(algorithm));
        EncryptingCredentials.IfPresent(credentials => builder.WithEncryptingCredentials(credentials));
    }
}

public sealed class JwtGenerationBuilder
{
    private bool _wasBuilt = false;
    private bool _hasSigningCredentials = false;
    private bool _hasLifetime = false;
    private readonly SecurityTokenDescriptor _descriptor;
    private readonly ISet<Claim> _claims = new HashSet<Claim>();

    public JwtGenerationBuilder(Instant issuedAt)
    {
        _descriptor = new SecurityTokenDescriptor
        {
            IssuedAt = issuedAt.ToDateTimeUtc()
        };
    }

    public SecurityTokenDescriptor Build()
    {
        if (_wasBuilt)
        {
            throw new InvalidOperationException("Cannot call Build() multiple times");
        }
        if (!_hasSigningCredentials)
        {
            throw new InvalidOperationException("Cannot build token without signing credentials");
        }
        if (!_hasLifetime)
        {
            throw new InvalidOperationException("Cannot build token without lifetime information");
        }

        _wasBuilt = true;
        _descriptor.Subject = new ClaimsIdentity(_claims);
        return _descriptor;
    }

    public JwtGenerationBuilder WithSigningCredentials(SigningCredentials signingCredentials)
    {
        _descriptor.SigningCredentials = signingCredentials;
        _hasSigningCredentials = true;
        return this;
    }

    public JwtGenerationBuilder WithLifetime(Duration lifetime)
    {
        _descriptor.NotBefore = _descriptor.IssuedAt;
        _descriptor.Expires = _descriptor.IssuedAt + lifetime.ToTimeSpan();
        _hasLifetime = true;
        return this;
    }

    public JwtGenerationBuilder WithIssuer(string issuer)
    {
        _descriptor.Issuer = issuer;
        return this;
    }

    public JwtGenerationBuilder WithAudience(string audience)
    {
        _descriptor.Audience = audience;
        return this;
    }

    public JwtGenerationBuilder WithEncryptingCredentials(EncryptingCredentials encryptingCredentials)
    {
        _descriptor.EncryptingCredentials = encryptingCredentials;
        return this;
    }

    public JwtGenerationBuilder WithCompressionAlgorithm(string compressionAlgorithm)
    {
        _descriptor.CompressionAlgorithm = compressionAlgorithm;
        return this;
    }
}
