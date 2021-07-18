using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    public record EventBusMessage(Guid Id, string EventType, string Content, Timestamp OccurredAt);
}
