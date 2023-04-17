using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.DesignTimeFactories;

internal class AuditingContextFactory : IDesignTimeDbContextFactory<AuditingContext>
{
    public AuditingContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<AuditingContext>();
        return new AuditingContext(options);
    }
}
