using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.DesignTimeFactories;

internal class AuthorizationContextFactory : IDesignTimeDbContextFactory<AuthorizationContext>
{
    public AuthorizationContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<AuthorizationContext>();
        return new AuthorizationContext(new NoTenant(), options);
    }
}
