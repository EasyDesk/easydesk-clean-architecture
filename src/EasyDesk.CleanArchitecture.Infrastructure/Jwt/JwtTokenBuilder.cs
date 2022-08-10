using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public delegate void JwtTokenConfiguration(JwtTokenBuilder builder);

public class JwtTokenBuilder
{
    private bool _wasBuilt = false;
    private bool _hasSigningCredentials = false;
    private bool _hasLifetime = false;
    private readonly SecurityTokenDescriptor _descriptor;
    private readonly ISet<Claim> _claims = new HashSet<Claim>();

    public JwtTokenBuilder(Instant issuedAt)
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

    public JwtTokenBuilder WithClaims(IEnumerable<Claim> claims) =>
        NextStep(() => _claims.UnionWith(claims));

    public JwtTokenBuilder WithClaim(Claim claim) =>
        NextStep(() => _claims.Add(claim));

    public JwtTokenBuilder WithSigningCredentials(SecurityKey key, string algorithm) =>
        NextStep(() =>
        {
            _descriptor.SigningCredentials = new SigningCredentials(key, algorithm);
            _hasSigningCredentials = true;
        });

    public JwtTokenBuilder WithLifetime(Duration lifetime) =>
        NextStep(() =>
        {
            _descriptor.NotBefore = _descriptor.IssuedAt;
            _descriptor.Expires = _descriptor.IssuedAt + lifetime.ToTimeSpan();
            _hasLifetime = true;
        });

    public JwtTokenBuilder WithIssuer(string issuer) =>
        NextStep(() => _descriptor.Issuer = issuer);

    public JwtTokenBuilder WithAudience(string audience) =>
        NextStep(() => _descriptor.Audience = audience);

    public JwtTokenBuilder WithEncryptingCredentials(EncryptingCredentials encryptingCredentials) =>
        NextStep(() => _descriptor.EncryptingCredentials = encryptingCredentials);

    public JwtTokenBuilder WithCompressionAlgorithm(string compressionAlgorithm) =>
        NextStep(() => _descriptor.CompressionAlgorithm = compressionAlgorithm);

    public JwtTokenBuilder WithAdditionalHeaderClaims(IDictionary<string, object> claims) =>
        NextStep(() => _descriptor.AdditionalHeaderClaims = claims);

    private JwtTokenBuilder NextStep(Action update)
    {
        update();
        return this;
    }
}
