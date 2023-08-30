using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static Option<T> GetServiceAsOption<T>(this IServiceProvider serviceProvider) where T : class =>
        serviceProvider.GetService<T>().AsOption();
}
