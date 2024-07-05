using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Commons.Collections;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public abstract class DomainContext : AbstractDbContext
{
    public const string SchemaName = "domain";

    protected DomainContext(DbContextOptions options)
        : base(options)
    {
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureDomainModel(modelBuilder);

        var versionedEntities = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.ClrType.AsOption())
            .Where(t => t.IsAssignableTo(typeof(IAggregateRootModel)))
            .ToList();

        if (versionedEntities.HasAny())
        {
            var genericConfigurationMethod = typeof(DomainContext).GetMethod(nameof(ConfigureVersionedEntity), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var args = new object[] { modelBuilder };
            versionedEntities
                .Select(t => genericConfigurationMethod.MakeGenericMethod(t))
                .ForEach(m => m.Invoke(this, args));
        }

        base.OnModelCreating(modelBuilder);
    }

    protected virtual void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }

    private void ConfigureVersionedEntity<E>(ModelBuilder modelBuilder)
        where E : class, IAggregateRootModel
    {
        var entityBuilder = modelBuilder.Entity<E>();
        entityBuilder.Property<long>(AggregateVersioningUtils.VersionPropertyName)
            .IsRequired()
            .IsConcurrencyToken();
    }
}
