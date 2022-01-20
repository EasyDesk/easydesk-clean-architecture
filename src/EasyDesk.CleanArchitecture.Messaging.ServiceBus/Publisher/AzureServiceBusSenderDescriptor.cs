using System;
using static EasyDesk.Tools.Functions;

namespace EasyDesk.CleanArchitecture.Messaging.ServiceBus.Publisher;

public abstract record AzureServiceBusSenderDescriptor
{
    private record QueueDescriptor(string QueueName) : AzureServiceBusSenderDescriptor;

    private record TopicDescriptor(string TopicName) : AzureServiceBusSenderDescriptor;

    public T Match<T>(Func<string, T> queue, Func<string, T> topic) => this switch
    {
        QueueDescriptor(var q) => queue(q),
        TopicDescriptor(var t) => topic(t),
        _ => throw new Exception("Invalid sender descriptor")
    };

    public void Match(Action<string> queue, Action<string> topic) => Match(
        queue: q => JustDoIt(() => queue(q)),
        topic: t => JustDoIt(() => topic(t)));

    public static AzureServiceBusSenderDescriptor Queue(string queueName) =>
        new QueueDescriptor(queueName);

    public static AzureServiceBusSenderDescriptor Topic(string topicName) =>
        new TopicDescriptor(topicName);
}
