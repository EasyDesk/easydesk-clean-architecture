using EasyDesk.Commons.Options;
using System.Diagnostics.CodeAnalysis;

namespace EasyDesk.Commons.Reflection;

public static class ReflectionUtils
{
    public static bool IsSubtypeOrImplementationOf(this Type type, Type otherType)
    {
        return type.IsGenericTypeDefinition || otherType.IsGenericTypeDefinition
            ? type.IsSubtypeOrImplementationOfOpenGeneric(otherType)
            : type.IsAssignableTo(otherType);
    }

    private static bool IsSubtypeOrImplementationOfOpenGeneric(this Type type, Type otherType)
    {
        if (type.GetInterfaces().Any(i => i.HasSameGenericTypeDefinitionAs(otherType)))
        {
            return true;
        }
        return type.IsSubtypeOfOpenGeneric(otherType);
    }

    private static bool IsSubtypeOfOpenGeneric(this Type type, Type otherType)
    {
        if (type.HasSameGenericTypeDefinitionAs(otherType))
        {
            return true;
        }
        return type.BaseType is Type baseType && baseType.IsSubtypeOfOpenGeneric(otherType);
    }

    private static bool HasSameGenericTypeDefinitionAs(this Type type, Type otherType) =>
        type.IsGenericType
            && otherType.IsGenericType
            && type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition();

    public static bool IsOneOf(this Type type, params Type[] possibleTypes)
    {
        return possibleTypes.Any(possibleType => possibleType == type);
    }

    public static bool IsAssignableTo(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type);
    }

    public static bool IsAssignableToOneOf(this Type type, params Type[] possibleBaseTypes)
    {
        return possibleBaseTypes.Any(possibleBaseType => possibleBaseType.IsAssignableFrom(type));
    }

    public static bool IsConstructedFrom(this Type type, Type genericType, [NotNullWhen(true)] out Type constructedType)
    {
        constructedType = new[] { type, }
            .Union(type.GetInheritanceChain())
            .Union(type.GetInterfaces())
            .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType)!;

        return constructedType is not null;
    }

    public static Option<Type> IsConstructedFrom(this Type type, Type genericType) =>
        TryOption<Type, Type, Type>(IsConstructedFrom, type, genericType);

    public static bool IsReferenceOrNullableType(this Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
    }

    public static object? GetDefaultValue(this Type type)
    {
        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
    }

    public static IEnumerable<Type> GetInheritanceChain(this Type type)
    {
        if (type.IsInterface)
        {
            return type.GetInterfaces();
        }

        var inheritanceChain = new List<Type>();

        var current = type;
        while (current.BaseType is not null)
        {
            inheritanceChain.Add(current.BaseType);
            current = current.BaseType;
        }

        return inheritanceChain;
    }
}
