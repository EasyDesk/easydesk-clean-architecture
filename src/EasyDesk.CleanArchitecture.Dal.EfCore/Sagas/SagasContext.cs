using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class SagasContext : AbstractDbContext<SagasContext>
{
    public SagasContext(ITenantProvider tenantProvider, DbContextOptions<SagasContext> options) : base(tenantProvider, options)
    {
    }

    public DbSet<SagaModel> Sagas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SagaManagerModel.SchemaName);

        modelBuilder.ApplyConfiguration(new SagaModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
