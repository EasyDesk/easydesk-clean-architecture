using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;

internal class MessagingContext : DbContext
{
    public const string SchemaName = "messaging";

    public MessagingContext(DbContextOptions<MessagingContext> options) : base(options)
    {
    }

    public DbSet<InboxMessage> Inbox { get; set; }

    public DbSet<OutboxMessage> Outbox { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfiguration(new InboxMessage.Configuration());
        modelBuilder.ApplyConfiguration(new OutboxMessage.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
