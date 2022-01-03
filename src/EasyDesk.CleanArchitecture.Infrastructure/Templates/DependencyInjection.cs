using EasyDesk.CleanArchitecture.Application.Templates;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;

namespace EasyDesk.CleanArchitecture.Infrastructure.Templates;

public static class DependencyInjection
{
    public static IServiceCollection AddRazorLightTemplateFactory(this IServiceCollection services, string path)
    {
        return services
            .AddScoped<ITemplateFactory, RazorLightTemplateFactory>()
            .AddSingleton(_ => CreateRazorLightEngine(path));
    }

    private static IRazorLightEngine CreateRazorLightEngine(string path)
    {
        return new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .UseFileSystemProject(path)
            .Build();
    }
}
