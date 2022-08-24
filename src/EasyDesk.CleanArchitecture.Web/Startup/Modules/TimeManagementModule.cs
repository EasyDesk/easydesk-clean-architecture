using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class TimeManagementModule : AppModule
{
    private readonly TimeManagementOptions _options;

    public TimeManagementModule(TimeManagementOptions options)
    {
        _options = options;
    }

    public IDateTimeZoneProvider DateTimeZoneProvider => _options.DateTimeZoneProvider;

    public IClock Clock => _options.Clock;

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(_ => Clock);
        services.AddSingleton(_ => DateTimeZoneProvider);
    }
}

public class TimeManagementOptions
{
    public IDateTimeZoneProvider DateTimeZoneProvider { get; set; } = DateTimeZoneProviders.Tzdb;

    public IClock Clock { get; set; } = SystemClock.Instance;
}

public static class TimeManagementModuleExtensions
{
    public static AppBuilder AddTimeManagement(this AppBuilder builder, Action<TimeManagementOptions> configure = null)
    {
        var options = new TimeManagementOptions();
        configure?.Invoke(options);
        return builder.AddModule(new TimeManagementModule(options));
    }
}
