using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreUnitOfWork : UnitOfWorkBase<DbTransaction>
{
    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

    public EfCoreUnitOfWork(DbTransaction transaction) : base(transaction)
    {
    }

    protected override async Task CommitTransaction()
    {
        await Transaction.CommitAsync();
    }

    protected override async Task RollbackTransaction() => await Transaction.RollbackAsync();

    public async Task EnlistDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }
        await dbContext.Database.UseTransactionAsync(Transaction);
        _registeredDbContexts.Add(dbContext);
    }
}
