using EasyDesk.Commons.Options;
using System.Diagnostics.CodeAnalysis;

namespace EasyDesk.Commons.Reflection;

public static class ReflectionUtils
{

    extension(Type type)
    {
        public bool IsSubtypeOrImplementationOf(Type otherType)
        {
            return type.IsGenericTypeDefinition || otherType.IsGenericTypeDefinition
                ? type.IsSubtypeOrImplementationOfOpenGeneric(otherType)
                : type.IsAssignableTo(otherType);
        }

        private bool IsSubtypeOrImplementationOfOpenGeneric(Type otherType)
        {
            if (type.GetInterfaces().Any(i => i.HasSameGenericTypeDefinitionAs(otherType)))
            {
                return true;
            }
            return type.IsSubtypeOfOpenGeneric(otherType);
        }

        private bool IsSubtypeOfOpenGeneric(Type otherType)
        {
            if (type.HasSameGenericTypeDefinitionAs(otherType))
            {
                return true;
            }
            return type.BaseType is Type baseType && baseType.IsSubtypeOfOpenGeneric(otherType);
        }

        private bool HasSameGenericTypeDefinitionAs(Type otherType) =>
            type.IsGenericType
                && otherType.IsGenericType
                && type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition();

        public bool IsOneOf(params Type[] possibleTypes)
        {
            return possibleTypes.Any(possibleType => possibleType == type);
        }

        public bool IsAssignableTo(Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        public bool IsAssignableToOneOf(params Type[] possibleBaseTypes)
        {
            return possibleBaseTypes.Any(possibleBaseType => possibleBaseType.IsAssignableFrom(type));
        }

        public bool IsConstructedFrom(Type genericType, [NotNullWhen(true)] out Type constructedType)
        {
            constructedType = new[] { type, }
                .Union(type.GetInheritanceChain())
                .Union(type.GetInterfaces())
                .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType)!;

            return constructedType is not null;
        }

        public Option<Type> IsConstructedFrom(Type genericType) =>
            TryOption<Type, Type, Type>(IsConstructedFrom, type, genericType);

        public bool IsReferenceOrNullableType =>
            !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

        public object? GetDefaultValue()
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        public IEnumerable<Type> GetInheritanceChain()
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

        public bool IsConcrete => !type.IsAbstract && !type.IsInterface;
    }
}
