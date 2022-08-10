using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public abstract class MultitenantDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public MultitenantDbContext(DbContextOptions options) : base(options)
    {
        try
        {
            _tenantProvider = this.GetService<ITenantProvider>();
        }
        catch
        {
            _tenantProvider = new NoTenant();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entityTypesWithinTenant = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.ClrType.AsOption())
            .Where(t => t.IsAssignableTo(typeof(IMultitenantEntity)))
            .ToList();

        if (entityTypesWithinTenant.Any())
        {
            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureEntityWithinTenant));
            var args = new object[] { modelBuilder };
            entityTypesWithinTenant
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }

        base.OnModelCreating(modelBuilder);
    }

    public void ConfigureEntityWithinTenant<T>(ModelBuilder modelBuilder)
        where T : class, IMultitenantEntity
    {
        var entityBuilder = modelBuilder.Entity<T>();

        entityBuilder.HasIndex(x => x.TenantId);

        entityBuilder.HasQueryFilter(x => x.TenantId == null || x.TenantId == _tenantProvider.TenantId.OrElseNull());
    }

    public override int SaveChanges()
    {
        SetTenantIdToAddedEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantIdToAddedEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetTenantIdToAddedEntities()
    {
        _tenantProvider.TenantId.IfPresent(tenantId =>
        {
            ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .Where(e => e.Entity is IMultitenantEntity)
            .ForEach(e => e.Property(nameof(IMultitenantEntity.TenantId)).CurrentValue = tenantId);
        });
    }
}
