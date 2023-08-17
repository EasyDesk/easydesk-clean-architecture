using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate.Events;

namespace EasyDesk.SampleApp.Application.DomainEvents;

public class PetCreatedHandler : IDomainEventHandler<PetCreatedEvent>
{
    private readonly ITenantNavigator _tenantNavigator;
    private readonly ITenantProvider _tenantProvider;

    public PetCreatedHandler(ITenantNavigator tenantNavigator, ITenantProvider tenantProvider)
    {
        _tenantNavigator = tenantNavigator;
        _tenantProvider = tenantProvider;
    }

    public Task<Result<Nothing>> Handle(PetCreatedEvent ev)
    {
        var tenant = _tenantProvider.Tenant;
        var contextTenant = _tenantNavigator.ContextTenant.Value;
        if (tenant != contextTenant)
        {
            return Task.FromResult<Result<Nothing>>(Errors.Generic("Domain handler tenant {tenant} doesn't match context tenant {contextTenant}.", tenant, contextTenant));
        }
        return Task.FromResult(Ok);
    }
}
