using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Domain;

public abstract class DomainContext<T> : AbstractDbContext<T>
    where T : DomainContext<T>
{
    public const string SchemaName = "domain";

    protected DomainContext(ITenantProvider tenantProvider, DbContextOptions<T> options)
        : base(tenantProvider, options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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
