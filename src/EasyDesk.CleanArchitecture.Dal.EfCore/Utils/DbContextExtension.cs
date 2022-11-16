using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public abstract class DbContextExtension
{
    protected DbContext Context { get; private set; }

    public void Initialize(DbContext context)
    {
        Context = context;
    }

    public abstract void ConfigureModel(ModelBuilder modelBuilder, QueryFiltersBuilder queryFilters, Action next);

    public abstract Task<int> SaveChanges(Func<Task<int>> next);
}
