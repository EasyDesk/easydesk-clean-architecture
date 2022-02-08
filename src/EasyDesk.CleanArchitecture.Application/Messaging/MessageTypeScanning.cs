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
            .SelectMany(GetHandledMessageTypes)
            .ToArray();
    }

    private static IEnumerable<Type> GetHandledMessageTypes(Type handlerType)
    {
        return handlerType.GetTypeInfo()
            .GetInterfaces()
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(IMessageHandler<>)))
            .Select(t => t.GetGenericArguments()[0]);
    }

    public static IEnumerable<Type> FindMessageTypes(params Type[] assemblyMarkers)
    {
        return ReflectionUtils.InstantiableSubtypesOf<IMessage>(assemblyMarkers);
    }
}
