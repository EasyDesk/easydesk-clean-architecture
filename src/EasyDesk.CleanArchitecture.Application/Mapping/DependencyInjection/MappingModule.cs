using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Application.Mapping.DependencyInjection;

public class MappingModule : IAppModule
{
    private readonly Type[] _additionalAssemblyMarkers;

    public MappingModule(Type[] additionalAssemblyMarkers)
    {
        _additionalAssemblyMarkers = additionalAssemblyMarkers;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAutoMapper(config =>
        {
            var userAsseblyMarkers = Items(
                app.ApplicationAssemblyMarker,
                app.WebAssemblyMarker,
                app.InfrastructureAssemblyMarker);

            config.AddProfile(new DefaultMappingProfile(_additionalAssemblyMarkers.Concat(userAsseblyMarkers)));
        });
    }
}

public static class MappingModuleExtensions
{
    public static AppBuilder AddMapping(this AppBuilder builder, params Type[] additionalAssemblyMarkers)
    {
        return builder.AddModule(new MappingModule(additionalAssemblyMarkers));
    }
}
