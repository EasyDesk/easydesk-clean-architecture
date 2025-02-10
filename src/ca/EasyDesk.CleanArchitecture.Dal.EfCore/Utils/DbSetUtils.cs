using EasyDesk.Commons.Options;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class DbSetUtils
{
    public static async Task<Option<T>> FindOptionAsync<T>(this DbSet<T> dbSet, params object[] keyValues) where T : class
    {
        var entity = await dbSet.FindAsync(keyValues);
        return entity.AsOption();
    }

    public static async Task<Option<T>> FindOptionAsync<T>(this DbSet<T> dbSet, object[] keyValues, CancellationToken cancellationToken) where T : class
    {
        var entity = await dbSet.FindAsync(keyValues, cancellationToken);
        return entity.AsOption();
    }
}
