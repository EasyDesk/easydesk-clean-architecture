using EasyDesk.CleanArchitecture.Application.Tenants;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Messaging.Receiver;

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
        _tenantInitializer.InitializeTenant(message.TenantId);
        return await _handler.Handle(message);
    }
}
