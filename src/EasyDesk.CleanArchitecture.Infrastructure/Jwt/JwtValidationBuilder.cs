using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public class JwtValidationBuilder<T>
{
    private readonly TokenValidationParameters _parameters;

    public JwtValidationBuilder(TokenValidationParameters parameters)
    {
        _parameters = parameters;
    }

    public JwtValidationBuilder<R> NextStep<R>(Action<TokenValidationParameters> configureParameters)
    {
        configureParameters(_parameters);
        return new(_parameters);
    }
}

public delegate JwtValidationBuilder<JwtValidationSteps.Final> JwtValidationConfiguration(
    JwtValidationBuilder<JwtValidationSteps.Initial> builder);

public static class JwtValidationSteps
{
    public record struct Initial;

    public record struct Final;

    public static JwtValidationBuilder<Final> WithSigningCredentials(this JwtValidationBuilder<Initial> builder, SecurityKey key) =>
        builder.NextStep<Final>(p =>
        {
            p.ValidateIssuerSigningKey = true;
            p.IssuerSigningKey = key;
        });

    public static JwtValidationBuilder<Final> WithoutLifetimeValidation(this JwtValidationBuilder<Final> builder) =>
        builder.NextStep<Final>(p => p.ValidateLifetime = false);

    public static JwtValidationBuilder<Final> WithIssuerValidation(this JwtValidationBuilder<Final> builder, IEnumerable<string> validIssuers) =>
        builder.NextStep<Final>(p =>
        {
            p.ValidIssuers = validIssuers;
            p.ValidateIssuer = true;
        });

    public static JwtValidationBuilder<Final> WithIssuerValidation(this JwtValidationBuilder<Final> builder, string validIssuer) =>
        builder.WithIssuerValidation(Items(validIssuer));

    public static JwtValidationBuilder<Final> WithAudienceValidation(this JwtValidationBuilder<Final> builder, IEnumerable<string> validAudiences) =>
        builder.NextStep<Final>(p =>
        {
            p.ValidAudiences = validAudiences;
            p.ValidateAudience = true;
        });

    public static JwtValidationBuilder<Final> WithAudienceValidation(this JwtValidationBuilder<Final> builder, string validAudience) =>
        builder.WithAudienceValidation(Items(validAudience));

    public static JwtValidationBuilder<Final> WithClockSkew(this JwtValidationBuilder<Final> builder, Duration skew) =>
        builder.NextStep<Final>(p => p.ClockSkew = skew.AsTimeSpan);

    public static JwtValidationBuilder<Final> WithDecryption(this JwtValidationBuilder<Final> builder, SecurityKey key) =>
        builder.WithDecryption(Items(key));

    public static JwtValidationBuilder<Final> WithDecryption(this JwtValidationBuilder<Final> builder, IEnumerable<SecurityKey> keys) =>
        builder.NextStep<Final>(p => p.TokenDecryptionKeys = keys);
}
