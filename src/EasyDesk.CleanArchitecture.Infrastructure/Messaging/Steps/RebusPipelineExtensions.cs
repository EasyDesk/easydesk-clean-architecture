using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

internal static class RebusPipelineExtensions
{
    public static void AddMultitenancySupport(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            var step = new TenantManagementStep();
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnSend(step, PipelineAbsolutePosition.Front)
                .OnReceive(step, PipelineAbsolutePosition.Front);
        });
    }

    public static void WrapHandlersInsideUnitOfWork(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnReceive(new UnitOfWorkStep(), PipelineAbsolutePosition.Front);
        });
    }

    public static void UseOutbox(this OptionsConfigurer configurer)
    {
        configurer.Decorate<ITransport>(c => new TransportWithOutbox(c.Get<ITransport>()));
    }

    public static void UseInbox(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepInjector(c.Get<IPipeline>())
                .OnReceive(new InboxStep(), PipelineRelativePosition.After, typeof(UnitOfWorkStep));
        });
    }

    public static void HandleDomainEventsAfterMessageHandlers(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnReceive(new DomainEventHandlingStep(), PipelineAbsolutePosition.Back);
        });
    }

    public static void OpenServiceScopeBeforeMessageHandlers(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnReceive(new ServiceScopeOpeningStep(), PipelineAbsolutePosition.Front);
        });
    }
}
