using EasyDesk.CleanArchitecture.Dal.EfCore.Auth;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.DesignTimeFactories;

internal class AuthorizationContextFactory : IDesignTimeDbContextFactory<AuthContext>
{
    public AuthContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<AuthContext>();
        return new(options);
    }
}
