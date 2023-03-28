using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;

public class JsonModule : AppModule
{
    private readonly JsonSettingsConfigurator? _configurator;

    public JsonModule(JsonSettingsConfigurator? configurator = null)
    {
        _configurator = configurator;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var dateTimeZoneProvider = app.RequireModule<TimeManagementModule>().DateTimeZoneProvider;
        services.AddSingleton<JsonSettingsConfigurator>(settings =>
        {
            ConfigureSettings(settings, dateTimeZoneProvider);
        });
    }

    public void ApplyJsonConfiguration(JsonSerializerSettings settings, AppDescription app)
    {
        var dateTimeZoneProvider = app.RequireModule<TimeManagementModule>().DateTimeZoneProvider;
        ConfigureSettings(settings, dateTimeZoneProvider);
    }

    private void ConfigureSettings(JsonSerializerSettings settings, IDateTimeZoneProvider dateTimeZoneProvider)
    {
        JsonDefaults.ApplyDefaultConfiguration(settings, dateTimeZoneProvider);
        _configurator?.Invoke(settings);
    }
}

public static class JsonModuleExtensions
{
    public static AppBuilder AddJsonSerialization(this AppBuilder builder, JsonSettingsConfigurator? configurator = null)
    {
        return builder.AddModule(new JsonModule(configurator));
    }
}
