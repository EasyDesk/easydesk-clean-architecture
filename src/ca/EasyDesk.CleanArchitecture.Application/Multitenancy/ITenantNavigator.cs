namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantNavigator : ITenantProvider
{
    void MoveToContextTenant();

    void MoveToTenant(TenantId id);

    void MoveToPublic();
}

public interface ITenantScope : IDisposable
{
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
}
