using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyDesk.SampleApp.Application.ExternalEventHandlers;

public class HandlePersonCreated : IMessageHandler<PersonCreated>
{
    private readonly ILogger<HandlePersonCreated> _logger;
    private readonly ITenantProvider _tenantProvider;

    public HandlePersonCreated(ILogger<HandlePersonCreated> logger, ITenantProvider tenantProvider)
    {
        _logger = logger;
        _tenantProvider = tenantProvider;
    }

    public Task Handle(PersonCreated message)
    {
        _logger.LogInformation("{message} for tenant {tenantId}", message, _tenantProvider.TenantId);
        return Task.CompletedTask;
    }
}
