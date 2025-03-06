using Autofac;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using NodaTime;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;

public class JsonModule : AppModule
{
    private readonly Action<JsonSerializerOptions, IComponentContext>? _configurator;

    public JsonModule(Action<JsonSerializerOptions, IComponentContext>? configurator = null)
    {
        _configurator = configurator;
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.Register<JsonOptionsConfigurator>(c => settings => ApplyJsonConfiguration(c, settings, app))
            .SingleInstance();
    }

    public void ApplyJsonConfiguration(IComponentContext context, JsonSerializerOptions options, AppDescription app)
    {
        var dateTimeZoneProvider = app.RequireModule<TimeManagementModule>().DateTimeZoneProvider;
        ConfigureSettings(context, options, dateTimeZoneProvider);
    }

    private void ConfigureSettings(IComponentContext context, JsonSerializerOptions options, IDateTimeZoneProvider dateTimeZoneProvider)
    {
        JsonDefaults.ApplyDefaultConfiguration(options, dateTimeZoneProvider);
        _configurator?.Invoke(options, context);
    }
}

public static class JsonModuleExtensions
{
    public static IAppBuilder AddJsonSerialization(this IAppBuilder builder)
    {
        return builder.AddJsonSerialization((_, _) => { });
    }

    public static IAppBuilder AddJsonSerialization(this IAppBuilder builder, Action<JsonSerializerOptions> configurator)
    {
        return builder.AddJsonSerialization((options, _) => configurator(options));
    }

    public static IAppBuilder AddJsonSerialization(this IAppBuilder builder, Action<JsonSerializerOptions, IComponentContext> configurator)
    {
        return builder.AddModule(new JsonModule(configurator));
    }
}
