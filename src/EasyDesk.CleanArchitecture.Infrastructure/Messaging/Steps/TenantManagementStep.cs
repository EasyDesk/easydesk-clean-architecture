using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Rebus.Messages;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public class TenantManagementStep : IOutgoingStep
{
    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        context.GetService<ITenantProvider>().TenantId.IfPresent(tenantId =>
        {
            context.Load<Message>().Headers.Add(MultitenantUtils.TenantIdHeader, tenantId);
        });
        await next();
    }
}
