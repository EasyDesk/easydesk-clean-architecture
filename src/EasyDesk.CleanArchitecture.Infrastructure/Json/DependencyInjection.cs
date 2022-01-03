using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Json.Converters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json;

public static class DependencyInjection
{
    public static IServiceCollection AddNewtonsoftJsonSerializer(this IServiceCollection services, Action<JsonSerializerSettings> settingsConfiguration = null)
    {
        return services.AddSingleton<IJsonSerializer>(_ =>
        {
            var settings = JsonDefaults.DefaultSerializerSettings();
            settingsConfiguration?.Invoke(settings);
            return new NewtonsoftJsonSerializer(settings);
        });
    }
}
