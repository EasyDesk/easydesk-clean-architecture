using EasyDesk.CleanArchitecture.Application.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Application.Tenants;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;

internal class TenantAwareMessageHandler : IMessageHandler
{
    private readonly IMessageHandler _handler;
    private readonly ITenantInitializer _tenantInitializer;

    public TenantAwareMessageHandler(IMessageHandler handler, ITenantInitializer tenantInitializer)
    {
        _handler = handler;
        _tenantInitializer = tenantInitializer;
    }

    public async Task<MessageHandlerResult> Handle(Message message)
    {
        _tenantInitializer.InitializeTenant(message.GetMetadata("TenantId")); // TODO: use a constant.
        return await _handler.Handle(message);
    }
}
