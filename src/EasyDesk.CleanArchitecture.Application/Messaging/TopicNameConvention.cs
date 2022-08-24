using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class TopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType) => eventType.Name;
}
