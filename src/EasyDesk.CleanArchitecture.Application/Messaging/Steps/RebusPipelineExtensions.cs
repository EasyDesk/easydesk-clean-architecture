using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

public static class RebusPipelineExtensions
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

    public static void WrapHandlersInsideTransaction(this OptionsConfigurer configurer)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            var step = new TransactionStep();
            return new PipelineStepConcatenator(c.Get<IPipeline>()).OnReceive(step, PipelineAbsolutePosition.Front);
        });
    }

    public static T GetScopedService<T>(this StepContext stepContext) =>
        stepContext.Load<ITransactionContext>().GetServiceProvider().GetRequiredService<T>();
}
