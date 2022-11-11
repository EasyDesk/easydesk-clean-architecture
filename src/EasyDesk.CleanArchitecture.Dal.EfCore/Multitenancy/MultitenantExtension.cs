using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public class MultitenantExtension : DbContextExtension
{
    private readonly ITenantProvider _tenantProvider;

    public MultitenantExtension(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override void CreateModel(ModelBuilder modelBuilder, Action next)
    {
        next();

        var multitenantEntities = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.ClrType.AsOption())
            .Where(t => t.IsAssignableTo(typeof(IMultitenantEntity)))
            .ToList();

        if (multitenantEntities.Any())
        {
            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureMultitenantEntity));
            var args = new object[] { modelBuilder };
            multitenantEntities
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }
    }

    public void ConfigureMultitenantEntity<E>(ModelBuilder modelBuilder)
        where E : class, IMultitenantEntity
    {
        var entityBuilder = modelBuilder.Entity<E>();

        entityBuilder.HasIndex(x => x.TenantId);

        entityBuilder.HasQueryFilter(x => x.TenantId == null || x.TenantId == _tenantProvider.TenantId.OrElseNull());
    }

    public override async Task<int> SaveChanges(AsyncFunc<int> next)
    {
        SetTenantIdToAddedEntities();
        return await next();
    }

    private void SetTenantIdToAddedEntities()
    {
        _tenantProvider.TenantId.IfPresent(tenantId =>
        {
            Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Where(e => e.Entity is IMultitenantEntity)
                .ForEach(e => e.CurrentValues[nameof(IMultitenantEntity.TenantId)] = tenantId);
        });
    }
}

public static class MultitenantExtensionExtensions
{
    public static void AddMultitenancy<T>(this AbstractDbContext<T> context, ITenantProvider tenantProvider)
        where T : AbstractDbContext<T>
    {
        context.AddExtension(new MultitenantExtension(tenantProvider));
    }
}
