using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class HttpContextModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddHttpContextAccessor();
    }
}

public static class HttpContextModuleExtensions
{
    public static AppBuilder AddHttpContext(this AppBuilder builder)
    {
        return builder.AddModule(new HttpContextModule());
    }
}
