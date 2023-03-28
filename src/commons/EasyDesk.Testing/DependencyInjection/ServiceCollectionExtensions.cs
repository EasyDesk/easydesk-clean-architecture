using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace EasyDesk.Testing.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static T AddSubstituteFor<T>(this IServiceCollection services, params object[] constructorArguments)
        where T : class
    {
        var substitute = Substitute.For<T>(constructorArguments);
        services.AddSingleton(substitute);
        return substitute;
    }
}
