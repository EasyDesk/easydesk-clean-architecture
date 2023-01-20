namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IContextTenantInitializer
{
    void Initialize(TenantInfo tenantInfo);

    bool IsInitialized { get; }
}
