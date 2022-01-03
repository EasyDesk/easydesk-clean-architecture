using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus;

public record EventBusMessage(
    Guid Id,
    Timestamp OccurredAt,
    string EventType,
    Option<string> TenantId,
    string Content);
