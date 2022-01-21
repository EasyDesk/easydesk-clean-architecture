using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Collections.EnumerableUtils;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;

public static class MessageBrokerTestingUtils
{
    public static Message NewDefaultMessage(ITimestampProvider timestampProvider = null) =>
        NewMessageWithType("MESSAGE_TYPE", timestampProvider);

    public static Message NewMessageWithType(string type, ITimestampProvider timestampProvider = null) =>
        new(Guid.NewGuid(), timestampProvider?.Now ?? Timestamp.Now, type, None, "{}");

    public static IEnumerable<Message> NewMessageSequence(int count, ITimestampProvider timestampProvider = null) =>
        Generate(() => NewDefaultMessage(timestampProvider)).Take(count).ToList();
}
