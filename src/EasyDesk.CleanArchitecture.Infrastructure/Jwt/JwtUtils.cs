using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Configuration;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public static class JwtUtils
{
    public static JwtSettings ReadJwtSettings(this IConfiguration configuration, string scopeName)
    {
        var scopeSection = configuration.RequireSection($"JwtScopes:{scopeName}");
        var lifetime = Duration.FromTimeSpan(scopeSection.RequireValue<TimeSpan>("Lifetime"));
        var key = KeyUtils.KeyFromString(scopeSection.RequireValue<string>("Key"));

        return new(lifetime, key);
    }
}
