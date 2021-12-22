using EasyDesk.CleanArchitecture.Application.Tenants;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Events.EventBus
{
    internal class TenantAwareEventBusMessageHandler : IEventBusMessageHandler
    {
        private readonly IEventBusMessageHandler _handler;
        private readonly ITenantInitializer _tenantInitializer;

        public TenantAwareEventBusMessageHandler(IEventBusMessageHandler handler, ITenantInitializer tenantInitializer)
        {
            _handler = handler;
            _tenantInitializer = tenantInitializer;
        }

        public async Task<EventBusMessageHandlerResult> Handle(EventBusMessage message)
        {
            _tenantInitializer.InitializeTenant(message.TenantId);
            return await _handler.Handle(message);
        }
    }
}
