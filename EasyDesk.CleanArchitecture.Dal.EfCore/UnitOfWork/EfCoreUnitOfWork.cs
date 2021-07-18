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

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork
{
    public class EfCoreUnitOfWork : UnitOfWorkBase<IDbContextTransaction>
    {
        private readonly DbContext _context;
        private readonly ISet<DbContext> _registeredDbContexts = new HashSet<DbContext>();

        public EfCoreUnitOfWork(DbContext context)
        {
            _context = context;
            _registeredDbContexts.Add(context);
        }

        protected override async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        protected override async Task<Response<Nothing>> SaveWithinTransaction(IDbContextTransaction transaction)
        {
            try
            {
                await _context.SaveChangesAsync();
                return Ok;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Errors.Generic(
                    Errors.Codes.Internal,
                    "A concurrency error occurred while saving changes to the database: {message}",
                    ex.Message);
            }
            catch (DbUpdateException ex)
            {
                return Errors.Generic(
                    Errors.Codes.Internal,
                    "An error occurred while saving changes to the database: {message}",
                    ex.Message);
            }
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
                    Errors.Codes.Internal,
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
}
