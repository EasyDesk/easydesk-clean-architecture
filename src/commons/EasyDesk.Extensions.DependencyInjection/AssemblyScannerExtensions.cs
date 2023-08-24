using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.DependencyInjection;

public static class AssemblyScannerExtensions
{
    public static void RegisterImplementationsAsSingleton(this IServiceCollection services, Type interfaceType, Func<AssemblyScanner, AssemblyScanner> scan) =>
        RegisterImplementations(scan, interfaceType, (i, t) => services.AddSingleton(i, t));

    public static void RegisterImplementationsAsScoped(this IServiceCollection services, Type interfaceType, Func<AssemblyScanner, AssemblyScanner> scan) =>
        RegisterImplementations(scan, interfaceType, (i, t) => services.AddScoped(i, t));

    public static void RegisterImplementationsAsTransient(this IServiceCollection services, Type interfaceType, Func<AssemblyScanner, AssemblyScanner> scan) =>
        RegisterImplementations(scan, interfaceType, (i, t) => services.AddTransient(i, t));

    private static void RegisterImplementations(
        Func<AssemblyScanner, AssemblyScanner> scan,
        Type interfaceType,
        Action<Type, Type> registerAction)
    {
        var types = scan(new AssemblyScanner())
            .SubtypesOrImplementationsOf(interfaceType)
            .NonAbstract()
            .NonGeneric()
            .FindTypes();
        if (interfaceType.IsGenericTypeDefinition)
        {
            types.ForEach(t =>
            {
                t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
                    .ForEach(i => registerAction(i, t));
            });
        }
        else
        {
            types.ForEach(t => registerAction(interfaceType, t));
        }
    }
}
