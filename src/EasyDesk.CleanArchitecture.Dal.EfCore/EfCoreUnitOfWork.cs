using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore;

public class EfCoreUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    public EfCoreUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public async Task Save() => await _context.SaveChangesAsync();
}
