using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;

public static class DispatcherExtensions
{
    public static IServiceCollection AddInherited<T>(this IServiceCollection services, Func<IServiceProvider, T> factory)
        where T : class
    {
        return services.AddScoped(sp =>
        {
            if (DispatcherScopeManager.Current is not null && DispatcherScopeManager.Current.RootServiceProvider != sp)
            {
                return DispatcherScopeManager.Current.RootServiceProvider.GetRequiredService<T>();
            }

            return factory(sp);
        });
    }
}
