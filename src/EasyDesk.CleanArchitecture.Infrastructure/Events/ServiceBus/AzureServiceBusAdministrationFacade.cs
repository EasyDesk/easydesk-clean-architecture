using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EasyDesk.Tools;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public class AzureServiceBusAdministrationFacade
    {
        private const string RuleName = "main";

        private readonly ServiceBusAdministrationClient _client;

        public AzureServiceBusAdministrationFacade(ServiceBusAdministrationClient client)
        {
            _client = client;
        }

        public async Task CreateQueueIdempotent(string queueName)
        {
            await TryCreate(
                () => CreateQueue(queueName),
                () => Task.CompletedTask);
        }

        private async Task CreateQueue(string queueName)
        {
            var queueOptions = new CreateQueueOptions(queueName);
            await _client.CreateQueueAsync(queueOptions);
        }

        public async Task CreateSubscriptionIdempotent(string topicName, string subscriptionName, SubscriptionFilter filter)
        {
            await TryCreate(
                () => CreateSubscription(topicName, subscriptionName, filter),
                () => UpdateSubscriptionRule(topicName, subscriptionName, filter));
        }

        private async Task CreateSubscription(string topicName, string subscriptionName, SubscriptionFilter filter)
        {
            var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscriptionName);
            var ruleOptions = new CreateRuleOptions(RuleName, filter.GetRuleFilter());
            await _client.CreateSubscriptionAsync(subscriptionOptions, ruleOptions);
        }

        private async Task UpdateSubscriptionRule(string topicName, string subscriptionName, SubscriptionFilter filter)
        {
            var ruleProperties = await _client.GetRuleAsync(topicName, subscriptionName, RuleName).Map(r => r.Value);
            ruleProperties.Filter = filter.GetRuleFilter();
            await _client.UpdateRuleAsync(topicName, subscriptionName, ruleProperties);
        }

        public async Task CreateTopicIdempotent(string topicName)
        {
            await TryCreate(
                () => CreateTopic(topicName),
                () => Task.CompletedTask);
        }

        private async Task CreateTopic(string topicName)
        {
            var topicOptions = new CreateTopicOptions(topicName);
            await _client.CreateTopicAsync(topicOptions);
        }

        private async Task TryCreate(AsyncAction create, AsyncAction update)
        {
            try
            {
                await create();
            }
            catch (ServiceBusException ex)
            {
                if (ex.Reason != ServiceBusFailureReason.MessagingEntityAlreadyExists)
                {
                    throw;
                }
                await update();
            }
        }
    }
}
