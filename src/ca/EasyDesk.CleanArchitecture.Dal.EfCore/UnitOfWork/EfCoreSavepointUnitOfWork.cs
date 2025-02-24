using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal sealed class EfCoreSavepointUnitOfWork : EfCoreUnitOfWork
{
    private readonly string _savepoint;

    public EfCoreSavepointUnitOfWork(string savepoint, DbTransaction transaction) : base(transaction)
    {
        _savepoint = savepoint;
    }

    public override Task Commit() => Task.CompletedTask;

    public override Task Rollback() => DbTransaction.RollbackAsync(_savepoint);

    public override void Dispose()
    {
    }
}
