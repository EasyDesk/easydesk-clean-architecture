using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.UnitTests.Application.Messaging.MessageBroker;

public static class MessageBrokerTestingUtils
{
    public static IEnumerable<Message> NewMessageSequence(int count) =>
        Generate(Message.CreateEmpty).Take(count).ToList();
}
