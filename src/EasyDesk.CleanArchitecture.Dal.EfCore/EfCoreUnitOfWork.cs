using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public EfCoreUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public async Task<Response<Nothing>> Save()
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
}
