using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class AuditingContext : AbstractDbContext
{
    public AuditingContext(DbContextOptions<AuditingContext> options)
        : base(options)
    {
    }

    public DbSet<AuditRecordModel> AuditRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuditModel.SchemaName);

        modelBuilder.ApplyConfiguration(new AuditRecordModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
