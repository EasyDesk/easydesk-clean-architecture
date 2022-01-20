using EasyDesk.CleanArchitecture.Application.Features;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class HttpContextFeature : IAppFeature
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddHttpContextAccessor();
    }
}

public static class HttpContextFeatureExtensions
{
    public static AppBuilder AddHttpContext(this AppBuilder builder)
    {
        return builder.AddFeature(new HttpContextFeature());
    }
}
