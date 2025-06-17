using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext : DbContext
{
    public const string PublicTenantName = "";

    private readonly IList<DbContextExtension> _extensions = [];

    public AbstractDbContext(DbContextOptions options) : base(options)
    {
    }

    protected void AddExtension(DbContextExtension extension)
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

        if (multitenantEntities.HasAny())
        {
            var genericConfigurationMethod = GetType().GetMethod(nameof(ConfigureMultitenantEntity))!;
            var args = new object[] { modelBuilder, queryFilters, };
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
        var tenantIdProperty = entityBuilder.Metadata.GetProperty(nameof(IMultitenantEntity.Tenant));
        if (!tenantIdProperty.IsIndex())
        {
            entityBuilder.HasIndex(x => x.Tenant);
        }
        entityBuilder.Property(x => x.Tenant)
            .IsRequired()
            .HasMaxLength(TenantId.MaxLength)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<TenantIdGenerator>();

        queryFilters.AddFilter<E>(x => x.Tenant == PublicTenantName
            || x.Tenant == GetCurrentTenantAsString()
            || this.GetService<ITenantProvider>().Tenant.IsPublic);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _extensions.Aggregate(
            () => base.SaveChangesAsync(cancellationToken),
            (curr, ext) => () => ext.SaveChanges(curr))();
    }

    private string GetCurrentTenantAsString() =>
        this.GetService<ITenantProvider>().Tenant.Id.Map(t => t.Value).OrElse(PublicTenantName);

    public class TenantIdGenerator : ValueGenerator
    {
        public override bool GeneratesTemporaryValues => false;

        public override bool GeneratesStableValues => true;

        protected override object? NextValue(EntityEntry entry) =>
            ((AbstractDbContext)entry.Context).GetCurrentTenantAsString();
    }
}
