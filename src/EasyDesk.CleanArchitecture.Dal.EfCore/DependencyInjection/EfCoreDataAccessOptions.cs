using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class EfCoreDataAccessOptions<T, TBuilder, TExtension>
    where T : DomainContext
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private Action<DbContextOptionsBuilder> _configureDbContextOptions;
    private Action<TBuilder> _configureProviderOptions;

    public EfCoreDataAccessOptions(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public bool ShouldApplyMigrations { get; private set; } = false;

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> ApplyMigrations(bool apply = true)
    {
        ShouldApplyMigrations = apply;
        return this;
    }

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> ConfigureDbContextOptions(Action<DbContextOptionsBuilder> configure)
    {
        _configureDbContextOptions += configure;
        return this;
    }

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> ConfigureProviderOptions(Action<TBuilder> configure)
    {
        _configureProviderOptions += configure;
        return this;
    }

    internal void ApplyDbContextOptions(DbContextOptionsBuilder options) =>
        _configureDbContextOptions?.Invoke(options);

    internal void ApplyProviderOptions(TBuilder options) =>
        _configureProviderOptions?.Invoke(options);
}
