using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreUnitOfWorkManager : UnitOfWorkManager<EfCoreUnitOfWork>
{
    private readonly DbConnection _connection;
    private int _savepointCounter = 0;

    public EfCoreUnitOfWorkManager(DbConnection connection)
    {
        _connection = connection;
    }

    public string SavePointName() => $"_savepoint_{++_savepointCounter}";

    public Option<EfCoreTransactionUnitOfWork> CurrentTransaction => MainUnitOfWork.Map(uow => (EfCoreTransactionUnitOfWork)uow);

    protected override async Task<EfCoreUnitOfWork> CreateTransaction()
    {
        if (MainUnitOfWork.IsAbsent(out var main))
        {
            if (_connection.State is ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
            var transaction = await _connection.BeginTransactionAsync();
            return new EfCoreTransactionUnitOfWork(transaction);
        }
        else
        {
            return new EfCoreSavepointUnitOfWork(SavePointName(), main.DbTransaction);
        }
    }

    public async Task EnlistDbContextForCurrentTransaction(DbContext context, bool failIfNoTransaction = false)
    {
        if (failIfNoTransaction && CurrentTransaction.IsAbsent)
        {
            throw new InvalidOperationException($"Unit of work was not started when registering DbContext of type {context.GetType()} to the transaction");
        }
        await CurrentTransaction.IfPresentAsync(uow => uow.EnlistDbContext(context));
    }

    protected override Task Commit(EfCoreUnitOfWork transaction) => transaction.Commit();

    protected override Task Rollback(EfCoreUnitOfWork transaction) => transaction.Rollback();
}
