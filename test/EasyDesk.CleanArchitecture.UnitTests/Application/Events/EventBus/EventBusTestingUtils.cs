using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Collections.EnumerableUtils;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Events.EventBus
{
    public static class EventBusTestingUtils
    {
        public static EventBusMessage NewDefaultMessage(ITimestampProvider timestampProvider = null) =>
            NewMessageWithType("EVENT_TYPE", timestampProvider);

        public static EventBusMessage NewMessageWithType(string eventType, ITimestampProvider timestampProvider = null) =>
            new(Guid.NewGuid(), timestampProvider?.Now ?? Timestamp.Now, eventType, None, "{}");

        public static IEnumerable<EventBusMessage> NewMessageSequence(int count, ITimestampProvider timestampProvider = null) =>
            Generate(() => NewDefaultMessage(timestampProvider))
                .Take(count)
                .ToList();
    }
}
