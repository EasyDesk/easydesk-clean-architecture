using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public abstract class DomainContext : MultitenantDbContext
{
    public const string SchemaName = "domain";

    protected DomainContext(DbContextOptions options) : base(options)
    {
    }

    protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureDomainModel(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    protected virtual void ConfigureDomainModel(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
