using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddApiKey(
        this AuthenticationModuleOptions options,
        string schemeName,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        var apiKeyOptions = new ApiKeyOptions();
        configureOptions?.Invoke(apiKeyOptions);
        return options.AddScheme(schemeName, new ApiKeyProvider(apiKeyOptions));
    }

    public static AuthenticationModuleOptions AddApiKey(
        this AuthenticationModuleOptions options,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        return options.AddApiKey(ApiKeyOptions.ApiKeyDefaultScheme, configureOptions);
    }
}
