using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

internal class TopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType) => eventType.Name;
}
