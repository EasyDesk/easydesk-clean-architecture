using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;

public class JsonModule : AppModule
{
    private readonly JsonOptionsConfigurator? _configurator;

    public JsonModule(JsonOptionsConfigurator? configurator = null)
    {
        _configurator = configurator;
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var dateTimeZoneProvider = app.RequireModule<TimeManagementModule>().DateTimeZoneProvider;
        services.AddSingleton<JsonOptionsConfigurator>(settings =>
        {
            ConfigureSettings(settings, dateTimeZoneProvider);
        });
    }

    public void ApplyJsonConfiguration(JsonSerializerOptions options, AppDescription app)
    {
        var dateTimeZoneProvider = app.RequireModule<TimeManagementModule>().DateTimeZoneProvider;
        ConfigureSettings(options, dateTimeZoneProvider);
    }

    private void ConfigureSettings(JsonSerializerOptions options, IDateTimeZoneProvider dateTimeZoneProvider)
    {
        JsonDefaults.ApplyDefaultConfiguration(options, dateTimeZoneProvider);
        _configurator?.Invoke(options);
    }
}

public static class JsonModuleExtensions
{
    public static IAppBuilder AddJsonSerialization(this IAppBuilder builder, JsonOptionsConfigurator? configurator = null)
    {
        return builder.AddModule(new JsonModule(configurator));
    }
}
