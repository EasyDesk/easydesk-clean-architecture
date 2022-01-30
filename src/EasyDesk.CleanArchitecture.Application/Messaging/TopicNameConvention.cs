using Rebus.Topic;
using System;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class TopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType) => eventType.Name;
}
