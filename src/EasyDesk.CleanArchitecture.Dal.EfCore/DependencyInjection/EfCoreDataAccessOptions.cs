using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class EfCoreDataAccessOptions<TBuilder, TExtension>
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private const string MigrationsTableName = "__EFMigrationsHistory";

    private readonly IEfCoreProvider<TBuilder, TExtension> _provider;
    private Action<DbContextOptionsBuilder> _configureDbContextOptions;
    private Action<TBuilder> _configureProviderOptions;

    public EfCoreDataAccessOptions(IEfCoreProvider<TBuilder, TExtension> provider)
    {
        _provider = provider;
    }

    internal Assembly InternalMigrationsAssembly => _provider.GetType().Assembly;

    public EfCoreDataAccessOptions<TBuilder, TExtension> ConfigureDbContextOptions(Action<DbContextOptionsBuilder> configure)
    {
        _configureDbContextOptions += configure;
        return this;
    }

    public EfCoreDataAccessOptions<TBuilder, TExtension> ConfigureProviderOptions(Action<TBuilder> configure)
    {
        _configureProviderOptions += configure;
        return this;
    }

    internal void RegisterUtilityServices(IServiceCollection services)
    {
        services.AddScoped(_ => _provider.NewConnection());
        services.AddScoped<TransactionEnlistingOnCommandInterceptor>();
        services.AddScoped<DbContextEnlistingOnSaveChangesInterceptor>();
    }

    internal void RegisterDbContext<C>(
        string schema,
        IServiceCollection services,
        Action<IServiceProvider, TBuilder> configure = null)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            var connection = provider.GetRequiredService<DbConnection>();
            _provider.ConfigureDbProvider(options, connection, relationalOptions =>
            {
                relationalOptions.MigrationsHistoryTable(MigrationsTableName, schema);
                _configureProviderOptions?.Invoke(relationalOptions);
                configure?.Invoke(provider, relationalOptions);
            });

            options.AddInterceptors(provider.GetRequiredService<TransactionEnlistingOnCommandInterceptor>());
            options.AddInterceptors(provider.GetRequiredService<DbContextEnlistingOnSaveChangesInterceptor>());
            _configureDbContextOptions?.Invoke(options);
        });
    }
}
