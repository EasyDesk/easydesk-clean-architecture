namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class PublicTenantProvider : ITenantProvider
{
    public TenantInfo TenantInfo => TenantInfo.Public;
}
