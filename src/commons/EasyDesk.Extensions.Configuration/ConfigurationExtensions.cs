using EasyDesk.Commons.Options;
using Microsoft.Extensions.Configuration;

namespace EasyDesk.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static Option<IConfigurationSection> GetSectionAsOption(this IConfiguration configuration, string key)
    {
        try
        {
            return Some(configuration.GetRequiredSection(key));
        }
        catch (InvalidOperationException)
        {
            return None;
        }
    }

    public static Option<string> GetConnectionStringAsOption(this IConfiguration configuration, string name) =>
        configuration.GetValueAsOption<string>(ConnectionStringKey(name));

    public static Option<T> GetValueAsOption<T>(this IConfiguration configuration, string key) where T : notnull =>
        configuration.GetSectionAsOption(key).FlatMap(s => s.Get<T>().AsOption());

    public static IConfigurationSection RequireSection(this IConfiguration configuration, string key) =>
        configuration.GetSectionAsOption(key).OrElseThrow(() => new MissingConfigurationException(GetCompleteKey(configuration, key)));

    public static string RequireConnectionString(this IConfiguration configuration, string name) =>
        configuration.RequireValue<string>(ConnectionStringKey(name));

    public static T RequireValue<T>(this IConfiguration configuration, string key) =>
        configuration.RequireSection(key).Get<T>() ?? throw new InvalidOperationException($"Section {key} couldn't bind to type {typeof(T)}.");

    private static string ConnectionStringKey(string name) => $"ConnectionStrings:{name}";

    private static string GetCompleteKey(IConfiguration configuration, string relativeKey) =>
        configuration switch
        {
            IConfigurationSection section => $"{section.Path}:{relativeKey}",
            _ => relativeKey
        };
}
