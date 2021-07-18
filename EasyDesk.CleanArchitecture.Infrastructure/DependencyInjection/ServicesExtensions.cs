using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static T AddConfigAsSingleton<T>(this IServiceCollection services, IConfiguration config)
            where T : class, new()
        {
            return AddConfigAsSingleton(services, config, () => new T());
        }

        public static T AddConfigAsSingleton<T>(this IServiceCollection services, IConfiguration config, Func<T> defaultValue)
            where T : class
        {
            var configSection = config.GetSection(typeof(T).Name).Get<T>();
            services.AddSingleton(configSection ?? defaultValue());
            return configSection;
        }
    }
}
