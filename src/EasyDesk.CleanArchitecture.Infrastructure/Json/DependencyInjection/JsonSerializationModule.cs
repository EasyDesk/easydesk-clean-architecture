using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json.DependencyInjection;

public class JsonSerializationModule : IAppModule
{
    private readonly Action<JsonSerializerSettings> _configureSettings;

    public JsonSerializationModule(Action<JsonSerializerSettings> configureSettings = null)
    {
        _configureSettings = configureSettings;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton<IJsonSerializer>(_ =>
        {
            var settings = JsonDefaults.DefaultSerializerSettings();
            ConfigureJsonSerializerSettings(settings);
            return new NewtonsoftJsonSerializer(settings);
        });
    }

    public void ConfigureJsonSerializerSettings(JsonSerializerSettings settings)
    {
        _configureSettings?.Invoke(settings);
    }
}

public static class NewtonsoftJsonSerializationModuleExtensions
{
    public static AppBuilder AddJsonSerialization(this AppBuilder builder, Action<JsonSerializerSettings> configureSettings = null)
    {
        return builder.AddModule(new JsonSerializationModule(configureSettings));
    }
}
