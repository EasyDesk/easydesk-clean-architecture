using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;

public class OutboxContext : ExtendedDbContext
{
    public const string SchemaName = "outbox";

    public OutboxContext(DbContextOptions<OutboxContext> options) : base(options)
    {
    }

    public DbSet<OutboxMessage> Messages { get; set; }

    protected override void SetupModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new OutboxMessage.Configuration());
    }
}
