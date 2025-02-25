using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal sealed class EfCoreTransactionUnitOfWork : EfCoreUnitOfWork
{
    public EfCoreTransactionUnitOfWork(DbTransaction transaction) : base(transaction)
    {
    }

    public override async Task Commit()
    {
        await DbTransaction.CommitAsync();
    }

    public override async Task Rollback()
    {
        await DbTransaction.RollbackAsync();
    }

    public override void Dispose()
    {
        DbTransaction.Dispose();
    }
}
