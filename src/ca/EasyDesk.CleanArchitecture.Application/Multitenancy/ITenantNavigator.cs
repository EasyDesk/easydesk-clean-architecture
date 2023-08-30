using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantNavigator : ITenantProvider
{
    void MoveToTenant(TenantId id);

    void MoveToPublic();
}

public static class TenantNavigatorExtensions
{
    public static Task SafeMoveTo(this ITenantNavigator navigator, TenantId id, IMultitenancyManager multitenancyManager) =>
        navigator.TryMoveTo(id, multitenancyManager).ThenThrowIfFailure();

    public static async Task<Result<Nothing>> TryMoveTo(this ITenantNavigator navigator, TenantId id, IMultitenancyManager multitenancyManager)
    {
        if (!await multitenancyManager.TenantExists(id))
        {
            return new TenantNotFoundError(id);
        }

        navigator.MoveToTenant(id);
        return Ok;
    }

    public static void MoveTo(this ITenantNavigator navigator, TenantInfo tenantInfo) =>
        tenantInfo.Id.Match(some: navigator.MoveToTenant, none: navigator.MoveToPublic);
}
