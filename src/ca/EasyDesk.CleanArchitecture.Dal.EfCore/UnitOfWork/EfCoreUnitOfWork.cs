using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal abstract class EfCoreUnitOfWork : IDisposable
{
    protected EfCoreUnitOfWork(DbTransaction dbTransaction)
    {
        DbTransaction = dbTransaction;
    }

    public DbTransaction DbTransaction { get; }

    public abstract Task Commit();

    public abstract Task Rollback();

    public abstract void Dispose();
}
