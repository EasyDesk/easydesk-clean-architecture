using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;

public class MappingModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAutoMapper(config =>
        {
            config.AddProfile(new DefaultMappingProfile(
                typeof(SupportedVersionDto),
                app.ApplicationAssemblyMarker,
                app.WebAssemblyMarker,
                app.InfrastructureAssemblyMarker));
        });
    }
}

public static class MappingModuleExtensions
{
    public static AppBuilder AddMapping(this AppBuilder builder)
    {
        return builder.AddModule(new MappingModule());
    }
}
