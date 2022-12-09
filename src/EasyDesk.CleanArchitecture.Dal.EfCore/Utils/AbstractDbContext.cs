using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext<T> : DbContext
    where T : AbstractDbContext<T>
{
    private readonly IList<DbContextExtension> _extensions = new List<DbContextExtension>();
    private readonly ITenantProvider _tenantProvider;

    public AbstractDbContext(ITenantProvider tenantProvider, DbContextOptions<T> options) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    internal void AddExtension(DbContextExtension extension)
    {
        extension.Initialize(this);
        _extensions.Add(extension);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var queryFilters = new QueryFiltersBuilder();
        _extensions.Aggregate(
            () => base.OnModelCreating(modelBuilder),
            (curr, ext) => () => ext.ConfigureModel(modelBuilder, queryFilters, curr))();

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

        queryFilters.ApplyToModelBuilder(modelBuilder);
    }

    public void ConfigureMultitenantEntity<E>(ModelBuilder modelBuilder, QueryFiltersBuilder queryFilters)
        where E : class, IMultitenantEntity
    {
        var entityBuilder = modelBuilder.Entity<E>();

        entityBuilder.HasIndex(x => x.TenantId);

        queryFilters.AddFilter<E>(x => x.TenantId == null
            || x.TenantId == _tenantProvider.TenantId.OrElseNull()
            || _tenantProvider.TenantId.IsAbsent);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdToAddedEntities();
        return await _extensions.Aggregate(
            () => base.SaveChangesAsync(cancellationToken),
            (curr, ext) => () => ext.SaveChanges(curr))();
    }

    private void SetTenantIdToAddedEntities()
    {
        _tenantProvider.TenantId.IfPresent(tenantId =>
        {
            ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added)
                .Where(e => e.Entity is IMultitenantEntity)
                .ForEach(e => e.CurrentValues[nameof(IMultitenantEntity.TenantId)] = tenantId);
        });
    }
}
