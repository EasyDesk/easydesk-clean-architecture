using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using EasyDesk.Commons.Reflection;

namespace EasyDesk.Extensions.DependencyInjection;

public static class RegistrationsExtensions
{
    public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> AssignableToOpenGenericType(
        this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> registration,
        Type type)
    {
        return registration.Where(t => t.IsSubtypeOrImplementationOf(type));
    }
}
