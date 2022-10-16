using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

public class TransactionEnlistingOnCommandInterceptor : DbCommandInterceptor
{
    private readonly EfCoreUnitOfWorkProvider _unitOfWorkProvider;

    public TransactionEnlistingOnCommandInterceptor(EfCoreUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        _unitOfWorkProvider.CurrentTransaction.IfPresent(t =>
        {
            result.Transaction = t.GetDbTransaction();
        });
        return result;
    }
}
