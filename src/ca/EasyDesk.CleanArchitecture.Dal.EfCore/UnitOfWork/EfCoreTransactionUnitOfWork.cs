using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal sealed class EfCoreTransactionUnitOfWork : EfCoreUnitOfWork
{
    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

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

    public async Task EnlistDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }
        await dbContext.Database.UseTransactionAsync(DbTransaction);
        _registeredDbContexts.Add(dbContext);
    }

    public override void Dispose()
    {
        DbTransaction.Dispose();
    }
}
