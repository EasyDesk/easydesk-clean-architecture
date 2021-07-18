using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Authentication.Jwt
{
    public static class JwtUtils
    {
        public static JwtScope FindJwtScope(this IServiceProvider provider, string scopeName)
        {
            var jwtService = provider.GetRequiredService<JwtService>();
            var config = provider.GetRequiredService<IConfiguration>();
            return CreateScopeFromNameAndSettings(jwtService, config, scopeName);
        }

        public static JwtScope CreateScopeFromNameAndSettings(JwtService jwtService, IConfiguration config, string scopeName)
        {
            var scopeSettings = config.GetSection($"AuthenticationSettings:JwtScopes:{scopeName}");
            return CreateScopeFromSettings(jwtService, scopeSettings);
        }

        private static JwtScope CreateScopeFromSettings(JwtService jwtService, IConfigurationSection settings)
        {
            var lifetime = Duration.FromTimeSpan(settings.GetValue<TimeSpan>("Lifetime"));
            var key = KeyUtils.KeyFromString(settings.GetValue<string>("Key"));

            return new JwtScope(jwtService, new(lifetime, key));
        }
    }
}
