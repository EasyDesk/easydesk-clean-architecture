using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class TransactionEnlistingOnCommandInterceptor : DbCommandInterceptor
{
    private readonly EfCoreUnitOfWorkManager _unitOfWorkManager;

    public TransactionEnlistingOnCommandInterceptor(EfCoreUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        _unitOfWorkManager.CurrentTransaction.IfPresent(t =>
        {
            result.Transaction = t;
        });
        return result;
    }
}
