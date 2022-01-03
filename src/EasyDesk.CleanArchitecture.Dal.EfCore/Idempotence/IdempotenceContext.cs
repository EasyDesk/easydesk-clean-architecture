using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;

public class IdempotenceContext : DbContext
{
    public const string SchemaName = "idempotence";

    public IdempotenceContext(DbContextOptions<IdempotenceContext> options) : base(options)
    {
    }

    public DbSet<HandledEvent> HandledEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new HandledEvent.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
