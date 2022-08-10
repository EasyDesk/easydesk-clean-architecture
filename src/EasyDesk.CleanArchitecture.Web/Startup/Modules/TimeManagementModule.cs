using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class TimeManagementModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton<IClock>(_ => SystemClock.Instance);
        services.AddSingleton(_ => DateTimeZoneProviders.Tzdb);
    }
}

public static class TimeManagementModuleExtensions
{
    public static AppBuilder AddTimeManagement(this AppBuilder builder)
    {
        return builder.AddModule(new TimeManagementModule());
    }
}
