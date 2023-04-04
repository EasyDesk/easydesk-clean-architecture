using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class AuditingContext : AbstractDbContext<AuditingContext>
{
    public AuditingContext(ITenantProvider tenantProvider, DbContextOptions<AuditingContext> options)
        : base(tenantProvider, options)
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
