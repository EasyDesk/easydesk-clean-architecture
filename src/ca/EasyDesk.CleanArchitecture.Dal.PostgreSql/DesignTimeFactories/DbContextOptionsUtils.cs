using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.DesignTimeFactories;

internal static class DbContextOptionsUtils
{
    public static DbContextOptions<T> CreateDesignTimeOptions<T>()
        where T : DbContext
    {
        return new DbContextOptionsBuilder<T>()
            .UseNpgsql(options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            })
            .Options;
    }
}
