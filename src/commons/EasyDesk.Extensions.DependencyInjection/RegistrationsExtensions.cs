using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;

namespace EasyDesk.Extensions.DependencyInjection;

public static class RegistrationsExtensions
{
    public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> AssignableToOpenGenericType<TLimit, TRegistrationStyle>(
        this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration,
        Type type)
    {
        return registration.Where(t => t.IsSubtypeOrImplementationOf(type));
    }

    public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AsParentsUpTo<TLimit, TActivatorData, TRegistrationStyle>(
        this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
        Type leaf,
        Type root)
    {
        return registration.As(ParentsOfTypeUpTo(leaf, root).ToArray());
    }

    private static IEnumerable<Type> ParentsOfTypeUpTo(Type leaf, Type root)
    {
        if (root.IsInterface)
        {
            throw new ArgumentException("Root cannot be an interface.", nameof(root));
        }

        if (!leaf.IsAssignableTo(root))
        {
            throw new ArgumentException("Leaf must be assignable to root.", nameof(leaf));
        }

        return EnumerableUtils
            .Iterate(leaf, t => t.BaseType!)
            .TakeWhile(x => x != root)
            .Append(root);
    }
}
