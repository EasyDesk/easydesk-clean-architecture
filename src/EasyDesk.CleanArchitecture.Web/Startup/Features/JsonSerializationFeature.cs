using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Infrastructure.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

internal class JsonSerializationFeature : IAppFeature
{
    private readonly Action<JsonSerializerSettings> _configureSettings;

    public JsonSerializationFeature(Action<JsonSerializerSettings> configureSettings = null)
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

public static class NewtonsoftJsonSerializationFeatureExtensions
{
    public static AppBuilder AddJsonSerialization(this AppBuilder builder, Action<JsonSerializerSettings> configureSettings = null)
    {
        return builder.AddFeature(new JsonSerializationFeature(configureSettings));
    }
}
