using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.DesignTimeFactories;

internal class AuditingContextFactory : IDesignTimeDbContextFactory<AuditingContext>
{
    public AuditingContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<AuditingContext>();
        return new AuditingContext(new PublicTenantProvider(), options);
    }
}
