using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;

public static class ExtensionUtils
{
    public static DbContextOptionsBuilder AddOrUpdateExtension<T>(this DbContextOptionsBuilder builder, T extension)
        where T : class, IDbContextOptionsExtension
    {
        (builder as IDbContextOptionsBuilderInfrastructure).AddOrUpdateExtension(extension);
        return builder;
    }
}
