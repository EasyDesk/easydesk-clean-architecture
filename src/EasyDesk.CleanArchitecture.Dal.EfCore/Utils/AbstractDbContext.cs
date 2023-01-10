using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.Tools.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext<T> : DbContext
    where T : AbstractDbContext<T>
{
    public const string PublicTenantName = "";

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

        var tenantIdProperty = entityBuilder.Metadata.GetProperty(nameof(IMultitenantEntity.TenantId));
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _extensions.Aggregate(
            () => base.SaveChangesAsync(cancellationToken),
            (curr, ext) => () => ext.SaveChanges(curr))();
    }

    private void SetTenantIdToAddedEntity(object entity)
    {
        if (entity is IMultitenantEntity e)
        {
            e.TenantId = GetCurrentTenantAsString();
        }
    }

    private void SetTenantIdToAddedEntities(IEnumerable<object> entities)
    {
        entities.ForEach(SetTenantIdToAddedEntity);
    }

    public override EntityEntry Add(object entity)
    {
        SetTenantIdToAddedEntity(entity);
        return base.Add(entity);
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        SetTenantIdToAddedEntity(entity);
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        SetTenantIdToAddedEntity(entity);
        return base.AddAsync(entity, cancellationToken);
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
    {
        SetTenantIdToAddedEntity(entity);
        return base.AddAsync(entity, cancellationToken);
    }

    public override void AddRange(IEnumerable<object> entities)
    {
        SetTenantIdToAddedEntities(entities);
        base.AddRange(entities);
    }

    public override void AddRange(params object[] entities)
    {
        SetTenantIdToAddedEntities(entities);
        base.AddRange(entities);
    }

    public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        SetTenantIdToAddedEntities(entities);
        return base.AddRangeAsync(entities, cancellationToken);
    }

    public override Task AddRangeAsync(params object[] entities)
    {
        SetTenantIdToAddedEntities(entities);
        return base.AddRangeAsync(entities);
    }

    private string GetCurrentTenantAsString() => _tenantProvider.TenantInfo.Id.Map(t => t.Value).OrElse(PublicTenantName);
}
