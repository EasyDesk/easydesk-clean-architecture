using Microsoft.Extensions.DependencyInjection;
using Rebus.Pipeline;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public static class StepContextExtensions
{
    public static T GetService<T>(this StepContext stepContext) where T : notnull =>
        stepContext.Load<ITransactionContext>().GetServiceProvider().GetRequiredService<T>();
}
