using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

public class EfCoreUnitOfWorkProvider : UnitOfWorkProviderBase<EfCoreUnitOfWork>
{
    private readonly DbContext _context;

    public EfCoreUnitOfWorkProvider(DbContext context)
    {
        _context = context;
    }

    public Option<IDbContextTransaction> CurrentTransaction => UnitOfWork.Map(uow => uow.Transaction);

    protected override async Task<EfCoreUnitOfWork> CreateUnitOfWork()
    {
        var transaction = await _context.Database.BeginTransactionAsync();
        var unitOfWork = new EfCoreUnitOfWork(_context, transaction);
        return unitOfWork;
    }

    public async Task EnlistDbContextForCurrentTransaction(DbContext context, bool failIfNoTransaction = false)
    {
        if (failIfNoTransaction && UnitOfWork.IsAbsent)
        {
            throw new InvalidOperationException($"Unit of work was not started when registering DbContext of type {context.GetType()} to the transaction");
        }
        await UnitOfWork.IfPresentAsync(uow => uow.EnlistDbContext(context));
    }
}
