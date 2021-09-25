namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusSettings
    {
        public string ConnectionString { get; set; }

        public string BasePath { get; set; }

        public string TopicName { get; set; }

        public string SubscriptionName { get; set; }

        public string CompleteTopicPath => $"{BasePath}/{TopicName}";
    }
}
