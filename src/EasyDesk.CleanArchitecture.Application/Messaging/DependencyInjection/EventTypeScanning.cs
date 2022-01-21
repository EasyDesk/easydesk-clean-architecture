using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public static class EventTypeScanning
{
    public static IEnumerable<Type> FindExternalEventTypes(params Type[] assemblyMarkers)
    {
        return ReflectionUtils.InstantiableTypesInAssemblies(assemblyMarkers)
            .SelectMany(t => GetEventType(t))
            .ToArray();
    }

    private static Option<Type> GetEventType(Type handlerType)
    {
        if (handlerType is null)
        {
            return None;
        }
        if (handlerType.IsGenericType && handlerType.GetGenericTypeDefinition().Equals(typeof(ExternalEventHandlerBase<>)))
        {
            return handlerType.GetGenericArguments().First();
        }
        return GetEventType(handlerType.BaseType);
    }
}
