using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class DbContextEnlistingOnSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly EfCoreUnitOfWorkProvider _unitOfWorkProvider;

    public DbContextEnlistingOnSaveChangesInterceptor(EfCoreUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWorkProvider.EnlistDbContextForCurrentTransaction(eventData.Context);
        return result;
    }
}
