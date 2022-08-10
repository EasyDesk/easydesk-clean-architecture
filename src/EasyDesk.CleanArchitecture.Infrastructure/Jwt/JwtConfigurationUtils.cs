using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public static class JwtConfigurationUtils
{
    public const string DefaultConfigurationSectionName = "JwtSettings";

    public static JwtValidationConfiguration GetJwtValidationConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var validationSection = section.GetSectionAsOption("Validation");
        var issuers = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>("ValidIssuers"));
        var audiences = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>("ValidAudiences"));

        return builder =>
        {
            builder.WithSignatureValidation(GetSecretKeyFromSection(section));
            issuers.IfPresent(i => builder.WithIssuerValidation(i));
            audiences.IfPresent(a => builder.WithAudienceValidation(a));
        };
    }

    public static JwtTokenConfiguration GetJwtTokenConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var authoritySection = section.RequireSection("Authority");
        var lifetime = Duration.FromTimeSpan(authoritySection.RequireValue<TimeSpan>("Lifetime"));
        var issuer = authoritySection.GetValueAsOption<string>("Issuer");
        var audience = authoritySection.GetValueAsOption<string>("Audience");
        return builder =>
        {
            builder
                .WithSigningCredentials(GetSecretKeyFromSection(section), SecurityAlgorithms.HmacSha256Signature)
                .WithLifetime(lifetime);
            issuer.IfPresent(i => builder.WithIssuer(i));
            audience.IfPresent(a => builder.WithAudience(a));
        };
    }

    private static SecurityKey GetSecretKeyFromSection(IConfigurationSection scopeSection) =>
        KeyUtils.KeyFromString(scopeSection.RequireValue<string>("SecretKey"));
}
