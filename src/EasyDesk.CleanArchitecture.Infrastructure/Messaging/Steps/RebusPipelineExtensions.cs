using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.ServiceProvider;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

internal static class RebusPipelineExtensions
{
    public static void UseOutbox(this OptionsConfigurer configurer)
    {
        configurer.Decorate<ITransport>(c => new TransportWithOutbox(c.Get<ITransport>()));
    }

    public static void OpenServiceScopeBeforeMessageHandlers(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepInjector(c.Get<IPipeline>())
                .OnReceive(new ServiceScopeOpeningStep(), PipelineRelativePosition.After, typeof(ServiceProviderProviderStep));
        });
    }
}
