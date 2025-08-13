using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.Sqlite.DesignTimeFactories;

internal static class DbContextOptionsUtils
{
    public static DbContextOptions<T> CreateDesignTimeOptions<T>()
        where T : DbContext
    {
        return new DbContextOptionsBuilder<T>()
            .UseSqlite(options =>
            {
                options.UseNodaTime();
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            })
            .Options;
    }
}
