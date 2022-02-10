using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using System;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public record KnownMessageTypes(IImmutableSet<Type> Types)
{
    public static KnownMessageTypes ScanAssemblies(params Type[] assemblyMarkers)
    {
        var messageTypes = ReflectionUtils
            .InstantiableSubtypesOf<IMessage>(assemblyMarkers)
            .ToEquatableSet();
        return new(messageTypes);
    }
}
