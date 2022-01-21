using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;

public class IdempotenceContext : ExtendedDbContext
{
    public const string SchemaName = "idempotence";

    public IdempotenceContext(DbContextOptions<IdempotenceContext> options) : base(options)
    {
    }

    public DbSet<HandledMessage> HandledMessages { get; set; }

    protected override void SetupModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new HandledMessage.Configuration());
    }
}
