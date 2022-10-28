using Rebus.Topic;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

internal class TopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType) => eventType.Name;
}
