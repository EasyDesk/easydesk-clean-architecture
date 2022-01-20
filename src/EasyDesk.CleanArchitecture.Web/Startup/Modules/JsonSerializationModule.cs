using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

internal class JsonSerializationModule : IAppModule
{
    private readonly Action<JsonSerializerSettings> _configureSettings;

    public JsonSerializationModule(Action<JsonSerializerSettings> configureSettings = null)
    {
        _configureSettings = configureSettings;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddNewtonsoftJsonSerializer(ConfigureJsonSerializerSettings);
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
