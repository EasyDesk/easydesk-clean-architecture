using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public abstract class EfCoreDataAccess<T, TBuilder, TExtension> : IDataAccessImplementation
    where T : DomainContext<T>
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private const string MigrationsTableName = "__EFMigrationsHistory";
    private readonly EfCoreDataAccessOptions<T, TBuilder, TExtension> _options;
    private readonly ISet<Type> _registeredDbContextTypes = new HashSet<Type>();

    public EfCoreDataAccess(EfCoreDataAccessOptions<T, TBuilder, TExtension> options)
    {
        _options = options;
    }

    public void AddMainDataAccessServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped(_ => CreateDbConnection(_options.ConnectionString));
        AddDbContext<T>(services, DomainContext<T>.SchemaName);
        services.AddScoped(provider => new EfCoreUnitOfWorkProvider(provider.GetRequiredService<DbConnection>()));
        services.AddScoped<IUnitOfWorkProvider>(provider => provider.GetRequiredService<EfCoreUnitOfWorkProvider>());
        services.AddScoped<TransactionEnlistingOnCommandInterceptor>();
        services.AddScoped<DbContextEnlistingOnSaveChangesInterceptor>();

        if (_options.ShouldApplyMigrations)
        {
            services.AddHostedService(p => new MigrationsHostedService(
                p.GetRequiredService<IServiceScopeFactory>(),
                _registeredDbContextTypes));
        }
    }

    protected abstract DbConnection CreateDbConnection(string connectionString);

    public void AddMessagingUtilities(IServiceCollection services, AppDescription app)
    {
        AddDbContext<MessagingContext>(
            services,
            MessagingContext.SchemaName,
            ConfigureMigrationsAssembly);
        services.AddScoped<IOutbox, EfCoreOutbox>();
        services.AddScoped<IInbox, EfCoreInbox>();
    }

    public void AddRoleBasedPermissionsProvider(IServiceCollection services, AppDescription app)
    {
        AddRoleManager(services, app);
        services.AddScoped<IPermissionsProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    public void AddRoleManager(IServiceCollection services, AppDescription app)
    {
        AddDbContext<AuthorizationContext>(
            services,
            AuthorizationContext.SchemaName,
            ConfigureMigrationsAssembly);
        services.AddScoped<EfCoreAuthorizationManager>();
        services.AddScoped<IUserRolesProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
        services.AddScoped<IUserRolesManager>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    private void ConfigureMigrationsAssembly(IServiceProvider provider, TBuilder relationalOptions)
    {
        relationalOptions.MigrationsAssembly(GetType().Assembly.GetName().Name);
    }

    private void AddDbContext<C>(
        IServiceCollection services,
        string schema,
        Action<IServiceProvider, TBuilder> configure = null)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            var connection = provider.GetRequiredService<DbConnection>();
            ConfigureDbProvider(options, connection, relationalOptions =>
            {
                relationalOptions.MigrationsHistoryTable(MigrationsTableName, schema);
                configure?.Invoke(provider, relationalOptions);
                _options.ApplyProviderOptions(relationalOptions);
            });
            options.AddInterceptors(provider.GetRequiredService<TransactionEnlistingOnCommandInterceptor>());
            options.AddInterceptors(provider.GetRequiredService<DbContextEnlistingOnSaveChangesInterceptor>());
            _options.ApplyDbContextOptions(options);
        });

        _registeredDbContextTypes.Add(typeof(C));
    }

    protected abstract void ConfigureDbProvider(DbContextOptionsBuilder options, DbConnection connection, Action<TBuilder> configure);
}

public static class EfCoreDataAccessExtensions
{
    public static AppBuilder AddEfCoreDataAccess<T, TBuilder, TExtension>(
        this AppBuilder builder,
        string connectionString,
        Func<EfCoreDataAccessOptions<T, TBuilder, TExtension>, EfCoreDataAccess<T, TBuilder, TExtension>> implementation,
        Action<EfCoreDataAccessOptions<T, TBuilder, TExtension>> configure = null)
        where T : DomainContext<T>
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension, new()
    {
        var options = new EfCoreDataAccessOptions<T, TBuilder, TExtension>(connectionString);
        configure?.Invoke(options);
        var dataAccessImplementation = implementation(options);
        return builder.AddDataAccess(dataAccessImplementation);
    }
}
