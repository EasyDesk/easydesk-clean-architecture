using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Application.Mapping.DependencyInjection;

public class MappingModule : AppModule
{
    private readonly Assembly[] _additionalAssemblies;

    public MappingModule(Assembly[] additionalAssemblies)
    {
        _additionalAssemblies = additionalAssemblies;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAutoMapper(config =>
        {
            var assemblies = Items(
                app.GetLayerAssembly(CleanArchitectureLayer.Infrastructure),
                app.GetLayerAssembly(CleanArchitectureLayer.Application),
                app.GetLayerAssembly(CleanArchitectureLayer.Web));

            config.AddProfile(new DefaultMappingProfile(assemblies.Concat(_additionalAssemblies)));
        });
    }
}

public static class MappingModuleExtensions
{
    public static AppBuilder AddMapping(this AppBuilder builder, params Assembly[] additionalAssemblies)
    {
        return builder.AddModule(new MappingModule(additionalAssemblies));
    }
}
