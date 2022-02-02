using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public static class MessageTypeScanning
{
    public static IEnumerable<Type> FindHandledMessageTypes(params Type[] assemblyMarkers)
    {
        return ReflectionUtils.InstantiableTypesInAssemblies(assemblyMarkers)
            .SelectMany(t => GetHandledMessageTypes(t))
            .ToArray();
    }

    private static IEnumerable<Type> GetHandledMessageTypes(Type handlerType)
    {
        if (handlerType is null)
        {
            return Enumerable.Empty<Type>();
        }
        return handlerType.GetTypeInfo()
            .GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(IMessageHandler<>)))
            .Select(t => t.GetGenericArguments()[0]);
    }

    public static IEnumerable<Type> FindMessageTypes(params Type[] assemblyMarkers)
    {
        return ReflectionUtils.InstantiableTypesInAssemblies(assemblyMarkers)
            .Where(t => t.IsAssignableTo(typeof(IMessage)));
    }
}
