using EasyDesk.CleanArchitecture.Application.Environment;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Environment;

public static class DependencyInjection
{
    public static IServiceCollection AddEnvironmentInfo(this IServiceCollection services)
    {
        return services.AddSingleton<IEnvironmentInfo, EnvironmentInfo>();
    }
}
