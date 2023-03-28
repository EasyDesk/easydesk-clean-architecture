namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public delegate Task<Result<TenantInfo>> MultitenantPolicy(
    TenantInfo contextTenantInfo,
    IMultitenancyManager multitenancyManager);

public static class MultitenantPolicies
{
    public static MultitenantPolicy IgnoreAndUsePublic() =>
        IgnoreExistence(_ => TenantInfo.Public);

    public static MultitenantPolicy IgnoreAndUseTenant(TenantId tenantId) =>
        IgnoreExistence(_ => TenantInfo.Tenant(tenantId));

    public static MultitenantPolicy IgnoreAndUseExistingTenant(TenantId tenantId) =>
        CheckingExistence(_ => TenantInfo.Tenant(tenantId));

    public static MultitenantPolicy AnyTenantOrPublic() =>
        IgnoreExistence(t => t);

    public static MultitenantPolicy ExistingTenantOrPublic() =>
        CheckingExistence(t => t);

    public static MultitenantPolicy RequirePublic() =>
        IgnoreExistence(t => Ensure(t, t => t.IsPublic, t => new MultitenancyNotSupportedError()));

    public static MultitenantPolicy RequireAnyTenant() =>
        IgnoreExistence(t => Ensure(t, t => t.IsInTenant, t => new MissingTenantError()));

    public static MultitenantPolicy RequireExistingTenant() =>
        CheckingExistence(t => Ensure(t, t => t.IsInTenant, t => new MissingTenantError()));

    private static MultitenantPolicy IgnoreExistence(Func<TenantInfo, Result<TenantInfo>> f) => (tenantInfo, _) =>
        Task.FromResult(f(tenantInfo));

    private static MultitenantPolicy CheckingExistence(Func<TenantInfo, Result<TenantInfo>> f) => (tenantInfo, m) =>
        f(tenantInfo).FlatMapAsync(i => RequireExistingTenant(i, m));

    private static Task<Result<TenantInfo>> RequireExistingTenant(
        TenantInfo tenantInfo,
        IMultitenancyManager multitenancyManager)
    {
        return Success(tenantInfo).FilterAsync(
            info => CheckExistanceOfTenant(multitenancyManager, info),
            info => new TenantNotFoundError(info.RequireId()));
    }

    private static async Task<bool> CheckExistanceOfTenant(IMultitenancyManager multitenancyManager, TenantInfo tenantInfo) =>
        tenantInfo.IsPublic || await multitenancyManager.TenantExists(tenantInfo.RequireId());
}
