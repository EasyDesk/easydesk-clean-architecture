using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.SoftDeletion;

public class SoftDeletionExtension : DbContextExtension
{
    public override void CreateModel(ModelBuilder modelBuilder, Action next)
    {
        var softDeletableEntities = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.ClrType.AsOption())
            .Where(t => t.IsAssignableTo(typeof(ISoftDeletable)))
            .ToList();

        if (softDeletableEntities.Any())
        {
            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureSoftDeletableEntity));
            var args = new object[] { modelBuilder };
            softDeletableEntities
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }
    }

    public void ConfigureSoftDeletableEntity<E>(ModelBuilder modelBuilder)
        where E : class, ISoftDeletable
    {
        modelBuilder.Entity<E>().HasQueryFilter(x => !x.IsDeleted);
    }

    public override async Task<int> SaveChanges(AsyncFunc<int> next)
    {
        Context.ChangeTracker.Entries()
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
