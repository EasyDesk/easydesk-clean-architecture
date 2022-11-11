using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.SqlServer.DesignTimeFactories;

internal static class DbContextOptionsUtils
{
    public static DbContextOptions<T> CreateDesignTimeOptions<T>()
        where T : DbContext
    {
        return new DbContextOptionsBuilder<T>()
            .UseSqlServer(options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            })
            .Options;
    }
}
