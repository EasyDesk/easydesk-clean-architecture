using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class TimeManagementModule : IAppModule
{
    private readonly IConfiguration _configuration;

    public TimeManagementModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTimestampProvider(_configuration);
    }
}

public static class TimeManagementModuleExtensions
{
    public static AppBuilder AddTimeManagement(this AppBuilder builder, IConfiguration configuration)
    {
        return builder.AddModule(new TimeManagementModule(configuration));
    }
}
