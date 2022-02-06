using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreUnitOfWorkProvider : UnitOfWorkProviderBase<EfCoreUnitOfWork>
{
    private readonly DbContext _dbContext;

    public EfCoreUnitOfWorkProvider(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task<EfCoreUnitOfWork> CreateUnitOfWork()
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync();
        var unitOfWork = new EfCoreUnitOfWork(_dbContext, transaction);
        return unitOfWork;
    }

    public async Task RegisterExternalDbContext(DbContext context) => await UnitOfWork
        .OrElseThrow(() => new InvalidOperationException($"Unit of work was not started when registering DbContext of type {context.GetType()} to the transaction"))
        .RegisterExternalDbContext(context);
}
