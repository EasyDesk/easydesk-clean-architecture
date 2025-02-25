using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class DbContextEnlistingOnSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly EfCoreUnitOfWorkManager _unitOfWorkManager;

    public DbContextEnlistingOnSaveChangesInterceptor(EfCoreUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWorkManager.EnlistDbContextForCurrentTransaction(eventData.Context!);
        return result;
    }
}
