using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;

internal class SagasContext : AbstractDbContext<SagasContext>
{
    public SagasContext(DbContextOptions<SagasContext> options)
        : base(options)
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
