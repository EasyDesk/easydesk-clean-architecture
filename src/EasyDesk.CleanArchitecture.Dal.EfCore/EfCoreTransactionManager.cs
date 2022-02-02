using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreTransactionManager : TransactionManagerBase<IDbContextTransaction>
{
    private readonly DbContext _context;
    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

    public EfCoreTransactionManager(DbContext context)
    {
        _context = context;
        _registeredDbContexts.Add(context);
    }

    protected override async Task<IDbContextTransaction> BeginTransaction()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    protected override async Task CommitTransaction(IDbContextTransaction transaction)
    {
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    protected override async Task RollbackTransaction(IDbContextTransaction transaction) => await transaction.RollbackAsync();

    public async Task RegisterExternalDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }
        await dbContext.Database.UseTransactionAsync(RequireTransaction().GetDbTransaction());
        _registeredDbContexts.Add(dbContext);
    }
}
