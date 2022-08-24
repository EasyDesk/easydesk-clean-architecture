using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreUnitOfWork : UnitOfWorkBase<IDbContextTransaction>
{
    private readonly DbContext _context;
    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

    public EfCoreUnitOfWork(DbContext context, IDbContextTransaction transaction) : base(transaction)
    {
        _context = context;
        _registeredDbContexts.Add(context);
    }

    protected override async Task CommitTransaction()
    {
        await _context.SaveChangesAsync();
        await Transaction.CommitAsync();
    }

    protected override async Task RollbackTransaction() => await Transaction.RollbackAsync();

    public async Task RegisterExternalDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }
        await dbContext.Database.UseTransactionAsync(Transaction.GetDbTransaction());
        _registeredDbContexts.Add(dbContext);
    }
}
