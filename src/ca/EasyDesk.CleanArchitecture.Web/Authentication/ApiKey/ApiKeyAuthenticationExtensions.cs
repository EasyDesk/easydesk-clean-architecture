using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddApiKey(
        this AuthenticationModuleOptions options,
        string schemeName,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        return options.AddScheme(schemeName, new ApiKeyProvider(new(() => new ApiKeyOptions().Also(configureOptions))));
    }

    public static AuthenticationModuleOptions AddApiKey(
        this AuthenticationModuleOptions options,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        return options.AddApiKey(ApiKeyOptions.ApiKeyDefaultScheme, configureOptions);
    }
}
