using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

public static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddApiKey(
        this AuthenticationModuleOptions options,
        string schemeName,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        return options.AddScheme(schemeName, new ApiKeyAuthenticationProvider(configureOptions));
    }

    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options,
        Action<ApiKeyOptions>? configureOptions = default)
    {
        return options.AddApiKey(ApiKeyOptions.ApiKeyDefaultScheme, configureOptions);
    }
}
