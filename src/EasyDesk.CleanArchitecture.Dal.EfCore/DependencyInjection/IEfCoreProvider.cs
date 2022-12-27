using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public interface IEfCoreProvider<TBuilder, TExtension>
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    DbConnection NewConnection();

    void ConfigureDbProvider(
        DbContextOptionsBuilder options,
        DbConnection connection,
        Action<TBuilder> configure);
}
