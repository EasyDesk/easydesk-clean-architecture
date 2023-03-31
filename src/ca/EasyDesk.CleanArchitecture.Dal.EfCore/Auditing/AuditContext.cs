using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class AuditContext : AbstractDbContext<AuditContext>
{
    public AuditContext(ITenantProvider tenantProvider, DbContextOptions<AuditContext> options)
        : base(tenantProvider, options)
    {
    }

    public DbSet<AuditRecordModel> Records { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AuditModel.SchemaName);

        modelBuilder.ApplyConfiguration(new AuditRecordModel.Configuration());

        base.OnModelCreating(modelBuilder);
    }
}
