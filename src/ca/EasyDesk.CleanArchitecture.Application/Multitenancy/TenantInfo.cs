using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record TenantInfo
{
    private TenantInfo(Option<TenantId> id)
    {
        Id = id;
    }

    public Option<TenantId> Id { get; }

    public bool IsPublic => Id.IsAbsent;

    public bool IsInTenant => Id.IsPresent;

    public static TenantInfo Public { get; } = new(None);

    public static TenantInfo Tenant(TenantId id) => new(Some(id));
}

public static class TenantInfoExtensions
{
    public static TenantId RequireId(this TenantInfo tenantInfo) => tenantInfo.Id
        .OrElseThrow(() => new InvalidOperationException("Attempted to read a tenant id while in the public tenant"));
}
