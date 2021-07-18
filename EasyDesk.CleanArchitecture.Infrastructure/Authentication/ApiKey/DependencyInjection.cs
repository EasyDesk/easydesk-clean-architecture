using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.ApiKey
{
    public static class DependencyInjection
    {
        public static AuthenticationSetupBuilder AddApiKeyAuth(
            this AuthenticationSetupBuilder builder,
            Action<ApiKeyAuthenticationOptions> options = null)
        {
            return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyDefaults.Scheme,
                options);
        }
    }
}
