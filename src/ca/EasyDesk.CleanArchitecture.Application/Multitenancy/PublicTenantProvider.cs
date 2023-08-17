namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class PublicTenantProvider : ITenantProvider
{
    public TenantInfo Tenant => TenantInfo.Public;
}
