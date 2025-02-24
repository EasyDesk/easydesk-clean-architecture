using Autofac;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public static class StepContextExtensions
{
    public static T Resolve<T>(this StepContext stepContext) where T : notnull =>
        stepContext.Load<ITransactionContext>().GetComponentContext().Resolve<T>();
}
