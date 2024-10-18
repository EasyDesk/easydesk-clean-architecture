using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Domain.Aggregates.PetAggregate.Events;

namespace EasyDesk.SampleApp.Application.DomainEvents;

public record PetCreatedError(TenantInfo Tenant, TenantInfo ContextTenant) : ApplicationError
{
    public override string GetDetail() => $"Domain handler tenant {Tenant} doesn't match context tenant {ContextTenant}.";
}

public class PetCreatedHandler : IDomainEventHandler<PetCreatedEvent>
{
    private readonly IContextTenantNavigator _tenantNavigator;

    public PetCreatedHandler(IContextTenantNavigator tenantNavigator)
    {
        _tenantNavigator = tenantNavigator;
    }

    public Task<Result<Nothing>> Handle(PetCreatedEvent ev)
    {
        var tenant = _tenantNavigator.Tenant;
        var contextTenant = _tenantNavigator.ContextTenant.Value;
        if (tenant != contextTenant)
        {
            return Task.FromResult<Result<Nothing>>(new PetCreatedError(tenant, contextTenant));
        }
        return Task.FromResult(Ok);
    }
}
