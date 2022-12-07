using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public interface ITenantSetter
{
    void SetTenant(Option<string> tenantId);
}

public class TenantService : ITenantProvider, ITenantSetter
{
    public void SetTenant(Option<string> tenantId) => TenantId = tenantId;

    public Option<string> TenantId { get; private set; }
}
