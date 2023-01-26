using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.SoftDeletion;

public class SoftDeletionExtension : DbContextExtension
{
    public override void ConfigureModel(ModelBuilder modelBuilder, QueryFiltersBuilder queryFilters, Action next)
    {
        next();

        var softDeletableEntities = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.ClrType.AsOption())
            .Where(t => t.IsAssignableTo(typeof(ISoftDeletable)))
            .ToList();

        if (softDeletableEntities.Any())
        {
            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureSoftDeletableEntity))!;
            var args = new object[] { queryFilters };
            softDeletableEntities
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }
    }

    public void ConfigureSoftDeletableEntity<E>(QueryFiltersBuilder queryFilters)
        where E : class, ISoftDeletable
    {
        queryFilters.AddFilter<E>(x => !x.IsDeleted);
    }

    public override async Task<int> SaveChanges(Func<Task<int>> next)
    {
        Context!.ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDeletable)
            .ForEach(e =>
            {
                switch (e.State)
                {
                    case EntityState.Deleted:
                        e.State = EntityState.Modified;
                        e.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = true;
                        break;
                    case EntityState.Added:
                        e.CurrentValues[nameof(ISoftDeletable.IsDeleted)] = false;
                        break;
                }
            });
        return await next();
    }
}

public static class SoftDeletionExtensionExtensions
{
    public static void AddSoftDeletion<T>(this AbstractDbContext<T> context)
        where T : AbstractDbContext<T>
    {
        context.AddExtension(new SoftDeletionExtension());
    }
}
