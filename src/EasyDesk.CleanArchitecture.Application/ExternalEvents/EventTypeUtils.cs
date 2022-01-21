using System;

namespace EasyDesk.CleanArchitecture.Application.ExternalEvents;

public static class EventTypeUtils
{
    public static string GetEventTypeName(this Type type) => type.Name;
}
