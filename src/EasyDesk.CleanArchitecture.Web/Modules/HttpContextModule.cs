using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Modules;

public class HttpContextModule : AppModule
{
    public override void ConfigureServices(IServiceCollection services, AppDescription app)
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
