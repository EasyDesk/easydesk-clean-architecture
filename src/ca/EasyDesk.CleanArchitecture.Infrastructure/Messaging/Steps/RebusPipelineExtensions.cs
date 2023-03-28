using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

internal static class RebusPipelineExtensions
{
    public static void UseOutbox(this OptionsConfigurer configurer)
    {
        configurer.Decorate<ITransport>(c => new TransportWithOutbox(c.Get<ITransport>()));
    }

    public static void SetupForMultitenancy(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnSend(new TenantManagementStep(), PipelineAbsolutePosition.Front);
        });
    }

    public static void PatchAsyncDisposables(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            return new PipelineStepConcatenator(c.Get<IPipeline>())
                .OnReceive(new ServiceScopeAsyncDisposeStep(), PipelineAbsolutePosition.Front);
        });
    }
}
