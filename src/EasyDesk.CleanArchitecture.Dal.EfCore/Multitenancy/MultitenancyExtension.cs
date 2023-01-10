using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

internal class MultitenancyExtension : DbContextExtension
{
    public const string PublicTenantName = "";
    private readonly ITenantProvider _tenantProvider;

    public MultitenancyExtension(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    private string GetCurrentTenantAsString() =>
        _tenantProvider.TenantInfo.Id.Map(t => t.Value).OrElse(PublicTenantName);

    public override void ConfigureModel(ModelBuilder modelBuilder, QueryFiltersBuilder queryFilters, Action next)
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
            var args = new object[] { modelBuilder, queryFilters };
            multitenantEntities
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }
    }

    public void ConfigureMultitenantEntity<E>(ModelBuilder modelBuilder, QueryFiltersBuilder queryFilters)
        where E : class, IMultitenantEntity
    {
        var entityBuilder = modelBuilder.Entity<E>();

        var tenantIdProperty = entityBuilder
            .Metadata
            .GetProperty(nameof(IMultitenantEntity.TenantId));
        if (!tenantIdProperty.IsIndex())
        {
            entityBuilder.HasIndex(x => x.TenantId);
        }
        entityBuilder.Property(x => x.TenantId)
                 .IsRequired()
                 .HasMaxLength(TenantId.MaxLength);

        queryFilters.AddFilter<E>(x => x.TenantId == PublicTenantName
            || x.TenantId == GetCurrentTenantAsString()
            || _tenantProvider.TenantInfo.IsPublic);
    }

    public override async Task<int> SaveChanges(Func<Task<int>> next)
    {
        Context.ChangeTracker.Entries()
            .Where(e => e.Entity is IMultitenantEntity)
            .ForEach(e =>
            {
                switch (e.State)
                {
                    case EntityState.Added:
                        e.CurrentValues[nameof(IMultitenantEntity.TenantId)] = GetCurrentTenantAsString();
                        break;
                }
            });
        return await next();
    }
}

public static class MultitenancyExtensionExtensions
{
    public static void AddMultitenancy<T>(this AbstractDbContext<T> context, ITenantProvider tenantProvider)
        where T : AbstractDbContext<T>
    {
        context.AddExtension(new MultitenancyExtension(tenantProvider));
    }
}
