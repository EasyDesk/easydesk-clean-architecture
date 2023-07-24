using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreSaveChangesHandler<T> : ISaveChangesHandler
    where T : DbContext
{
    private readonly T _context;

    public EfCoreSaveChangesHandler(T context)
    {
        _context = context;
    }

    public async Task SaveChanges()
    {
        if (_context.ChangeTracker.HasChanges())
        {
            await _context.SaveChangesAsync();
        }
    }
}
