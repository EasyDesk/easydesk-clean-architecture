namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantInfo(Option<TenantId> Id)
{
    public bool IsPublic => Id.IsAbsent;

    public bool IsInTenant => Id.IsPresent;

    public static TenantInfo Public { get; } = new(None);

    public static TenantInfo Tenant(TenantId id) => new(Some(id));
}
