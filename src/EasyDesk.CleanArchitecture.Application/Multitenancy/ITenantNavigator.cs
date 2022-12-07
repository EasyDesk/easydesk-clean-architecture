namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantNavigator : ITenantProvider
{
    void MoveTo(string tenantId);

    void MoveToNoTenant();

    void BackToContextTenant();
}

public static class TenantNavigatorExtensions
{
    public static async Task SafeMoveTo(this ITenantNavigator navigator, string tenantId, IMultitenancyManager multitenancyManager)
    {
        await navigator.TryMoveTo(tenantId, multitenancyManager).ThenThrowIfFailure();
    }

    public static async Task<Result<Nothing>> TryMoveTo(this ITenantNavigator navigator, string tenantId, IMultitenancyManager multitenancyManager)
    {
        if (!await multitenancyManager.TenantExists(tenantId))
        {
            return new TenantNotFoundError(tenantId);
        }

        navigator.MoveTo(tenantId);
        return Ok;
    }
}
