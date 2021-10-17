using System;
using static EasyDesk.Tools.Functions;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public abstract record AzureServiceBusReceiverDescriptor
    {
        private record QueueDescriptor(string QueueName) : AzureServiceBusReceiverDescriptor;

        private record SubscriptionDescriptor(string TopicName, string SubscriptionName) : AzureServiceBusReceiverDescriptor;

        public T Match<T>(Func<string, T> queue, Func<string, string, T> subscription) => this switch
        {
            QueueDescriptor(var q) => queue(q),
            SubscriptionDescriptor(var t, var s) => subscription(t, s),
            _ => throw new Exception("Invalid receiver descriptor")
        };

        public void Match(Action<string> queue, Action<string, string> subscription) => Match(
            queue: q => JustDoIt(() => queue(q)),
            subscription: (t, s) => JustDoIt(() => subscription(t, s)));

        public static AzureServiceBusReceiverDescriptor Queue(string queueName) =>
            new QueueDescriptor(queueName);

        public static AzureServiceBusReceiverDescriptor Subscription(string topicName, string subscriptionName) =>
            new SubscriptionDescriptor(topicName, subscriptionName);
    }
}
