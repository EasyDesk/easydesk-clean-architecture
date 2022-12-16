using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public static class JwtConfigurationUtils
{
    public const string DefaultConfigurationSectionName = "JwtSettings";
    public const string DefaultAuthoritySectionName = "Authority";
    public const string DefaultLifetimeKeyName = "Lifetime";
    public const string DefaultIssuerKeyName = "Issuer";
    public const string DefaultAudienceKeyName = "Audience";
    public const string DefaultSecretSigningKeyKeyName = "SecretKey";
    public const string DefaultValidationAudiencesKeyName = "ValidAudiences";
    public const string DefaultValidationIssuersKeyName = "ValidIssuers";
    public const string DefaultValidationSectionName = "Validation";

    public static JwtValidationConfiguration GetJwtValidationConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var validationSection = section.GetSectionAsOption(DefaultValidationSectionName);
        var issuers = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>(DefaultValidationIssuersKeyName));
        var audiences = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>(DefaultValidationAudiencesKeyName));

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
        var authoritySection = section.RequireSection(DefaultAuthoritySectionName);
        var lifetime = Duration.FromTimeSpan(authoritySection.RequireValue<TimeSpan>(DefaultLifetimeKeyName));
        var issuer = authoritySection.GetValueAsOption<string>(DefaultIssuerKeyName);
        var audience = authoritySection.GetValueAsOption<string>(DefaultAudienceKeyName);
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
        KeyUtils.KeyFromString(scopeSection.RequireValue<string>(DefaultSecretSigningKeyKeyName));
}
