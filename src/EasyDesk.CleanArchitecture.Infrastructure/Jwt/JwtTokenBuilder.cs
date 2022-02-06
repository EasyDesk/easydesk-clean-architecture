using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using static EasyDesk.CleanArchitecture.Infrastructure.Jwt.JwtTokenBuilderSteps;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public class JwtTokenBuilder<T>
{
    private readonly SecurityTokenDescriptor _token;

    public JwtTokenBuilder(SecurityTokenDescriptor token)
    {
        _token = token;
    }

    public JwtTokenBuilder<R> NextStep<R>(Action<SecurityTokenDescriptor> updateToken)
    {
        updateToken(_token);
        return new(_token);
    }
}

public delegate JwtTokenBuilder<Final> JwtTokenConfiguration(
    JwtTokenBuilder<Initial> builder);

public static class JwtTokenBuilderSteps
{
    public record struct Initial;

    public record struct Lifetime;

    public record struct Issuer;

    public record struct Audience;

    public record struct Final;

    public static JwtTokenBuilder<Lifetime> WithSigningCredentials(this JwtTokenBuilder<Initial> builder, SecurityKey key, string algorithm) =>
        builder.NextStep<Lifetime>(t => t.SigningCredentials = new SigningCredentials(key, algorithm));

    public static JwtTokenBuilder<Issuer> WithLifetime(this JwtTokenBuilder<Lifetime> builder, Duration lifetime) =>
        builder.NextStep<Issuer>(t =>
        {
            t.NotBefore = t.IssuedAt;
            t.Expires = t.IssuedAt + lifetime.AsTimeSpan;
        });

    public static JwtTokenBuilder<Audience> WithIssuer(this JwtTokenBuilder<Issuer> builder, string issuer) =>
        builder.NextStep<Audience>(t => t.Issuer = issuer);

    public static JwtTokenBuilder<Final> WithAudience(this JwtTokenBuilder<Audience> builder, string audience) =>
        builder.NextStep<Final>(t => t.Audience = audience);

    public static JwtTokenBuilder<Final> WithEncryptingCredentials(this JwtTokenBuilder<Final> builder, EncryptingCredentials encryptingCredentials) =>
        builder.NextStep<Final>(t => t.EncryptingCredentials = encryptingCredentials);

    public static JwtTokenBuilder<Final> WithCompressionAlgorithm(this JwtTokenBuilder<Final> builder, string compressionAlgorithm) =>
        builder.NextStep<Final>(t => t.CompressionAlgorithm = compressionAlgorithm);

    public static JwtTokenBuilder<Final> WithAdditionalHeaderClaims(this JwtTokenBuilder<Final> builder, IDictionary<string, object> claims) =>
        builder.NextStep<Final>(t => t.AdditionalHeaderClaims = claims);
}
