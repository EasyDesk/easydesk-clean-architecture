using Microsoft.Extensions.Configuration;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static IConfigurationSection RequireSection(this IConfiguration configuration, string key)
    {
        try
        {
            return configuration.GetRequiredSection(key);
        }
        catch (InvalidOperationException)
        {
            throw new MissingConfigurationException(GetCompleteKey(configuration, key));
        }
    }

    public static string RequireConnectionString(this IConfiguration configuration, string name) =>
        configuration.RequireValue<string>($"ConnectionStrings:{name}");

    public static T RequireValue<T>(this IConfiguration configuration, string key) =>
        configuration.RequireSection(key).Get<T>();

    private static string GetCompleteKey(IConfiguration configuration, string relativeKey) =>
        configuration switch
        {
            IConfigurationSection section => $"{section.Path}:{relativeKey}",
            _ => relativeKey
        };
}
