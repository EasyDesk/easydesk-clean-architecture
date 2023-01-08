using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.SampleApp.Application.IncomingCommands;

public record CreateTenant(string Id) : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.IgnoreAndUseTenant(TenantId.Create(Id));
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
        await _multitenancyManager.AddTenant(_tenantProvider.TenantInfo.Id.Value);
        return Ok;
    }
}
