using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

public class TransactionEnlistingInterceptor : DbCommandInterceptor
{
    private readonly EfCoreUnitOfWorkProvider _unitOfWorkProvider;

    public TransactionEnlistingInterceptor(EfCoreUnitOfWorkProvider unitOfWorkProvider)
    {
        _unitOfWorkProvider = unitOfWorkProvider;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        _unitOfWorkProvider.UnitOfWork
            .Map(uow => uow.Transaction.GetDbTransaction())
            .IfPresent(t => result.Transaction = t);
        return result;
    }
}
