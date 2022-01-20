using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup.Features;

public class MappingFeature : IAppFeature
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

public static class MappingFeatureExtensions
{
    public static AppBuilder AddMapping(this AppBuilder builder)
    {
        return builder.AddFeature(new MappingFeature());
    }
}
