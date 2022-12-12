namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantProvider
{
    TenantInfo TenantInfo { get; }
}
