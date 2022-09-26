using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

public class EfCoreUnitOfWorkProvider : UnitOfWorkProviderBase<EfCoreUnitOfWork>
{
    private readonly DbContext _context;

    public EfCoreUnitOfWorkProvider(DbContext context)
    {
        _context = context;
    }

    protected override async Task<EfCoreUnitOfWork> CreateUnitOfWork()
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        var unitOfWork = new EfCoreUnitOfWork(_context, transaction);
        return unitOfWork;
    }

    public async Task EnlistDbContextForCurrentTransaction(DbContext context) => await UnitOfWork
        .OrElseThrow(() => new InvalidOperationException($"Unit of work was not started when registering DbContext of type {context.GetType()} to the transaction"))
        .EnlistDbContext(context);
}
