using EasyDesk.CleanArchitecture.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;

internal class EfCoreSaveChangesHandler : ISaveChangesHandler
{
    private readonly ISet<DbContext> _contexts = new HashSet<DbContext>();

    public void AddDbContext(DbContext context)
    {
        _contexts.Add(context);
    }

    public async Task SaveChanges()
    {
        foreach (var context in _contexts)
        {
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
