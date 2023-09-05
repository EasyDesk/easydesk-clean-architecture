using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

public record CreateTenant(string Id) : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUseTenant(new TenantId(Id));
}

public class CreateTenantHandler : IHandler<CreateTenant>
{
    private readonly IMultitenancyManager _multitenancyManager;
    private readonly ITenantProvider _tenantProvider;

    public CreateTenantHandler(IMultitenancyManager multitenancyManager, ITenantProvider tenantProvider)
    {
        _multitenancyManager = multitenancyManager;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<Nothing>> Handle(CreateTenant request)
    {
        await _multitenancyManager.AddTenant(_tenantProvider.Tenant.Id.Value);
        return Ok;
    }
}
