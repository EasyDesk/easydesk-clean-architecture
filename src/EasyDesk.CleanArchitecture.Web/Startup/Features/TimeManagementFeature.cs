using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Infrastructure.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class TimeManagementFeature : IAppFeature
{
    private readonly IConfiguration _configuration;

    public TimeManagementFeature(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddTimestampProvider(_configuration);
    }
}

public static class TimeManagementFeatureExtensions
{
    public static AppBuilder AddTimeManagement(this AppBuilder builder, IConfiguration configuration)
    {
        return builder.AddFeature(new TimeManagementFeature(configuration));
    }
}
