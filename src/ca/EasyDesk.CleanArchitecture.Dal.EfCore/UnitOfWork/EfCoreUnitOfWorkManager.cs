using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreUnitOfWorkManager : UnitOfWorkManager<EfCoreUnitOfWork>
{
    private readonly DbConnection _connection;
    private int _savepointCounter;

    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

    public EfCoreUnitOfWorkManager(DbConnection connection)
    {
        _connection = connection;
    }

    public string SavePointName() => $"_savepoint_{++_savepointCounter}";

    public Option<DbTransaction> CurrentTransaction => CurrentUnitOfWork.Map(uow => uow.DbTransaction);

    protected override async Task<EfCoreUnitOfWork> CreateTransaction()
    {
        if (CurrentTransaction.IsAbsent(out var transaction))
        {
            if (_connection.State is ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
            transaction = await _connection.BeginTransactionAsync();
            return new EfCoreTransactionUnitOfWork(transaction);
        }
        var savepoint = SavePointName();
        await transaction.SaveAsync(savepoint);

        return new EfCoreSavepointUnitOfWork(savepoint, transaction);
    }

    public async Task EnlistDbContextForCurrentTransaction(DbContext context, bool failIfNoTransaction = false)
    {
        if (failIfNoTransaction && CurrentTransaction.IsAbsent)
        {
            throw new InvalidOperationException($"Unit of work was not started when registering DbContext of type {context.GetType()} to the transaction");
        }
        await EnlistDbContext(context);
    }

    protected override Task Commit(EfCoreUnitOfWork transaction) => transaction.Commit();

    protected override Task Rollback(EfCoreUnitOfWork transaction) => transaction.Rollback();

    private async Task EnlistDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }
        await CurrentTransaction.IfPresentAsync(async transaction =>
        {
            await dbContext.Database.UseTransactionAsync(transaction);
            _registeredDbContexts.Add(dbContext);
        });
    }
}
