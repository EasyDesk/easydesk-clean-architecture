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
}
