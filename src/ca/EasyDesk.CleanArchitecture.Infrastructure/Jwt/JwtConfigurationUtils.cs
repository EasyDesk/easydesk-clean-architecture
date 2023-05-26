using EasyDesk.Commons.Collections;
using EasyDesk.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System.Collections.Immutable;

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
    public const string DefaultAlgorithm = SecurityAlgorithms.HmacSha256Signature;

    public static JwtValidationConfiguration GetJwtValidationConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var validationSection = section.GetSectionAsOption(DefaultValidationSectionName);

        IImmutableSet<string> GetSet(string key) =>
            validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>(key)).Flatten().ToEquatableSet();

        var issuers = GetSet(DefaultValidationIssuersKeyName);
        var audiences = GetSet(DefaultValidationAudiencesKeyName);

        return new JwtValidationConfiguration(
            GetSecretKeyFromSection(section),
            issuers,
            audiences,
            DecryptionKeys: Enumerable.Empty<SecurityKey>());
    }

    public static JwtGenerationConfiguration GetJwtGenerationConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var authoritySection = section.RequireSection(DefaultAuthoritySectionName);
        var lifetime = Duration.FromTimeSpan(authoritySection.RequireValue<TimeSpan>(DefaultLifetimeKeyName));
        var issuer = authoritySection.GetValueAsOption<string>(DefaultIssuerKeyName);
        var audience = authoritySection.GetValueAsOption<string>(DefaultAudienceKeyName);

        return new JwtGenerationConfiguration(
            new SigningCredentials(
                GetSecretKeyFromSection(section),
                DefaultAlgorithm),
            lifetime,
            issuer,
            audience);
    }

    private static SecurityKey GetSecretKeyFromSection(IConfigurationSection scopeSection) =>
        KeyUtils.KeyFromString(scopeSection.RequireValue<string>(DefaultSecretSigningKeyKeyName));
}
