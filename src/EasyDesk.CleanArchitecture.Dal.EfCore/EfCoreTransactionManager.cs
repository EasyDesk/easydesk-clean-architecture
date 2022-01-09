using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreTransactionManager : TransactionManagerBase<IDbContextTransaction>
{
    private readonly DbContext _context;
    private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

    public EfCoreTransactionManager(DbContext context)
    {
        _context = context;
        _registeredDbContexts.Add(context);
    }

    protected override async Task<IDbContextTransaction> BeginTransaction()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    protected override async Task<Response<Nothing>> CommitTransaction(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.CommitAsync();
            return Ok;
        }
        catch (Exception ex)
        {
            return Errors.Generic(
                "An error occurred while committing a transaction to the database: {message}",
                ex.Message);
        }
    }

    public async Task RegisterExternalDbContext(DbContext dbContext)
    {
        if (_registeredDbContexts.Contains(dbContext))
        {
            return;
        }

        await dbContext.Database.UseTransactionAsync(RequireTransaction().GetDbTransaction());
        _registeredDbContexts.Add(dbContext);
    }
}
