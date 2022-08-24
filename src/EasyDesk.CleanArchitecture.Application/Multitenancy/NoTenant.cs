namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class NoTenant : ITenantProvider
{
    public Option<string> TenantId => None;
}
