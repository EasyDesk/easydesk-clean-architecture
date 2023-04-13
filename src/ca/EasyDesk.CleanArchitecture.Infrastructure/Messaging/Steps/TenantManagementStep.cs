using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Rebus.Messages;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;

public sealed class TenantManagementStep : IOutgoingStep
{
    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        context.GetService<ITenantProvider>().TenantInfo.Id.IfPresent(tenantId =>
        {
            context.Load<Message>().Headers.Add(MultitenantMessagingUtils.TenantIdHeader, tenantId);
        });
        await next();
    }
}
