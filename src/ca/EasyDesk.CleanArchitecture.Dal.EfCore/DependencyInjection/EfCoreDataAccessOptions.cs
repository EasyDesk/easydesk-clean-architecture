using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public sealed class EfCoreDataAccessOptions<T, TBuilder, TExtension>
    where T : AbstractDbContext
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private readonly IEfCoreProvider<TBuilder, TExtension> _provider;
    private Action<DbContextOptionsBuilder>? _configureDbContextOptions;
    private Action<TBuilder>? _configureProviderOptions;
    private Action<IServiceCollection>? _configureServices;

    public EfCoreDataAccessOptions(IEfCoreProvider<TBuilder, TExtension> provider)
    {
        _provider = provider;
    }

    internal Assembly InternalMigrationsAssembly => _provider.GetType().Assembly;

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

    public EfCoreDataAccessOptions<T, TBuilder, TExtension> WithService<S>()
        where S : class
    {
        _configureServices += services => services.AddScoped(p => p.GetRequiredService<T>() as S
            ?? throw new InvalidOperationException(
                $"Cannot use {typeof(S).Name} as a service type for DbContext {typeof(T).Name}."));
        return this;
    }

    internal void RegisterUtilityServices(IServiceCollection services)
    {
        services.AddScoped(_ => _provider.NewConnection());
        services.AddScoped<TransactionEnlistingOnCommandInterceptor>();
        services.AddScoped<DbContextEnlistingOnSaveChangesInterceptor>();
        _configureServices?.Invoke(services);
    }

    internal void RegisterDbContext<C>(
        IServiceCollection services,
        Action<IServiceProvider, TBuilder>? configure = null)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            var connection = provider.GetRequiredService<DbConnection>();
            _provider.ConfigureDbProvider(options, connection, relationalOptions =>
            {
                configure?.Invoke(provider, relationalOptions);
                _configureProviderOptions?.Invoke(relationalOptions);
            });

            options.AddInterceptors(provider.GetRequiredService<TransactionEnlistingOnCommandInterceptor>());
            options.AddInterceptors(provider.GetRequiredService<DbContextEnlistingOnSaveChangesInterceptor>());
            _configureDbContextOptions?.Invoke(options);
        });
    }
}
