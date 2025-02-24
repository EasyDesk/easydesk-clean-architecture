using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class TransactionEnlistingOnCommandInterceptor : DbCommandInterceptor
{
    private readonly EfCoreUnitOfWorkManager _unitOfWorkProvider;

    public TransactionEnlistingOnCommandInterceptor(EfCoreUnitOfWorkManager unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        _unitOfWorkProvider.CurrentTransaction.IfPresent(t =>
        {
            result.Transaction = t.DbTransaction;
        });
        return result;
    }
}
