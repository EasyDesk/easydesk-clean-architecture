using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Tools.Collections;
using Rebus.Messages;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Steps;

public class TenantManagementStep : IOutgoingStep, IIncomingStep
{
    public const string TenantIdHeader = "x-tenant-id";

    public async Task Process(OutgoingStepContext context, Func<Task> next)
    {
        context.GetScopedService<ITenantProvider>().TenantId.IfPresent(tenantId =>
        {
            context.Load<Message>().Headers.Add(TenantIdHeader, tenantId);
        });
        await next();
    }

    public async Task Process(IncomingStepContext context, Func<Task> next)
    {
        var tenantId = context.Load<TransportMessage>().Headers.GetOption(TenantIdHeader);
        context.GetScopedService<ITenantInitializer>().InitializeTenant(tenantId);

        await next();
    }
}
