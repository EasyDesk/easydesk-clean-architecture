namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public delegate Task<Result<TenantInfo>> MultitenantPolicy(
    TenantInfo contextTenantInfo,
    IMultitenancyManager multitenancyManager);

public static class MultitenantPolicies
{
    private static MultitenantPolicy Pure(Func<TenantInfo, Result<TenantInfo>> f) =>
        (id, _) => Task.FromResult(f(id));

    public static MultitenantPolicy Public() => Pure(_ => TenantInfo.Public);

    public static MultitenantPolicy Any(bool requireExisting = true) => (t, m) =>
        RequireExistingTenant(t, m, requireExisting);

    public static MultitenantPolicy RequireTenant(bool requireExisting = true) => (t, m) =>
        RequireExistingTenant(t, m, requireExisting).ThenFlatTap(t => Ensure(t.IsInTenant, () => new MissingTenantError()));

    public static MultitenantPolicy RequirePublic() =>
        Pure(t => Ensure(t.IsPublic, () => new MultitenancyNotSupportedError()).Map(_ => TenantInfo.Public));

    private static async Task<Result<TenantInfo>> RequireExistingTenant(
        TenantInfo tenantInfo,
        IMultitenancyManager multitenancyManager,
        bool requireExisting = true)
    {
        if (requireExisting && tenantInfo.IsInTenant && !await multitenancyManager.TenantExists(tenantInfo.Id.Value))
        {
            return new TenantNotFoundError(tenantInfo.Id.Value);
        }
        return tenantInfo;
    }
}
