using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreUnitOfWorkProvider : UnitOfWorkProviderBase<EfCoreUnitOfWork>
{
    private readonly DbConnection _connection;

    public EfCoreUnitOfWorkProvider(DbConnection connection)
    {
        _connection = connection;
    }

    public Option<DbTransaction> CurrentTransaction => UnitOfWork.Map(uow => uow.Transaction);

    protected override async Task<EfCoreUnitOfWork> CreateUnitOfWork()
    {
        if (_connection.State is ConnectionState.Closed)
        {
            await _connection.OpenAsync();
        }
        var transaction = await _connection.BeginTransactionAsync();
        return new EfCoreUnitOfWork(transaction);
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
