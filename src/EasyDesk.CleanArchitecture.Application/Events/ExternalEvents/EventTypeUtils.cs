using System;

namespace EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;

public static class EventTypeUtils
{
    public static string GetEventTypeName(this Type type) => type.Name;
}
