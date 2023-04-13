using Microsoft.Extensions.DependencyInjection;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class ServiceScopeAsyncDisposeStep : IIncomingStep
{
    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        await next();
        var scope = context.Load<IServiceScope>();
        if (scope is IAsyncDisposable asyncScope)
        {
            await asyncScope.DisposeAsync();
        }
    }
}
