namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface ITenantNavigator : ITenantProvider
{
    ITenantScope MoveToTenant(TenantId id);

    ITenantScope MoveToPublic();
}

public interface ITenantScope : IDisposable
{
}

public static class TenantNavigatorExtensions
{
    public static Task<ITenantScope> SafeMoveTo(this ITenantNavigator navigator, TenantId id, IMultitenancyManager multitenancyManager) =>
        navigator.TryMoveTo(id, multitenancyManager).ThenThrowIfFailure();

    public static async Task<Result<ITenantScope>> TryMoveTo(this ITenantNavigator navigator, TenantId id, IMultitenancyManager multitenancyManager)
    {
        if (!await multitenancyManager.TenantExists(id))
        {
            return new TenantNotFoundError(id);
        }

        return Success(navigator.MoveToTenant(id));
    }
}
