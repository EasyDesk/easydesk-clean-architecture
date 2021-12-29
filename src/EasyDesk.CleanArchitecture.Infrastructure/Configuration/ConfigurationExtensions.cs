using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Infrastructure.Configuration
{
    public static class ConfigurationExtensions
    {
        public static string GetRequiredConnectionString(this IConfiguration configuration, string name) =>
            configuration.GetRequiredValue<string>($"ConnectionStrings:{name}");

        public static IConfigurationSection GetRequiredSection(this IConfiguration configuration, string key) =>
            configuration.FindSection(SplitKey(key), GetCompleteKey(configuration, key));

        public static T GetRequiredValue<T>(this IConfiguration configuration, string key) =>
            configuration.GetRequiredSection(key).Get<T>();

        private static IConfigurationSection FindSection(this IConfiguration configuration, IImmutableQueue<string> keys, string completeKey)
        {
            if (keys.IsEmpty)
            {
                throw new ArgumentException($"Invalid configuration key '{completeKey}'");
            }
            var tail = keys.Dequeue(out var key);
            var subSection = configuration.GetChildren()
                .FirstOption(s => s.Key == key)
                .OrElseThrow(() => new MissingConfigurationException(completeKey));
            return tail.IsEmpty ? subSection : subSection.FindSection(tail, completeKey);
        }

        private static string GetCompleteKey(IConfiguration configuration, string relativeKey) =>
            configuration switch
            {
                IConfigurationSection section => $"{section.Path}:{relativeKey}",
                _ => relativeKey
            };

        private static IImmutableQueue<string> SplitKey(this string key) => ImmutableQueue.Create(key.Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }
}
