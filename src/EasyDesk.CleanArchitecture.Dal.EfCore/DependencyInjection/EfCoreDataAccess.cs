using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Tools.Collections;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class EfCoreDataAccess<T> : IDataAccessImplementation
    where T : DomainContext
{
    private readonly string _connectionString;
    private readonly bool _applyMigrations;
    private readonly Action<DbContextOptionsBuilder> _addtionalOptions;
    private readonly List<Type> _registeredDbContextTypes = new();

    public EfCoreDataAccess(
        string connectionString,
        bool applyMigrations = false,
        Action<DbContextOptionsBuilder> addtionalOptions = null)
    {
        _connectionString = connectionString;
        _applyMigrations = applyMigrations;
        _addtionalOptions = addtionalOptions;
    }

    public void AddMainDataAccessServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped(_ => new SqlConnection(_connectionString));
        AddDbContext<T>(services, DomainContext.SchemaName);
        services.AddScoped(provider => new EfCoreUnitOfWorkProvider(provider.GetRequiredService<T>()));
        services.AddScoped<IUnitOfWorkProvider>(provider => provider.GetRequiredService<EfCoreUnitOfWorkProvider>());
        services.AddScoped<TransactionEnlistingOnCommandInterceptor>();
        services.AddScoped<DbContextEnlistingOnSaveChangesInterceptor>();
    }

    public void AddMessagingUtilities(IServiceCollection services, AppDescription app)
    {
        AddDbContext<MessagingContext>(services, MessagingContext.SchemaName, ConfigureAsSecondaryDbContext);
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
        AddDbContext<AuthorizationContext>(services, AuthorizationContext.SchemaName, ConfigureAsSecondaryDbContext);
        services.AddScoped<EfCoreAuthorizationManager>();
        services.AddScoped<IUserRolesProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
        services.AddScoped<IUserRolesManager>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    private void ConfigureAsSecondaryDbContext(IServiceProvider provider, DbContextOptionsBuilder options)
    {
        options.AddInterceptors(provider.GetRequiredService<TransactionEnlistingOnCommandInterceptor>());
        options.AddInterceptors(provider.GetRequiredService<DbContextEnlistingOnSaveChangesInterceptor>());
    }

    private void AddDbContext<C>(IServiceCollection services, string schema, Action<IServiceProvider, DbContextOptionsBuilder> configure = null)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            ConfigureDbContextOptions(provider, options, schema);
            configure?.Invoke(provider, options);
            _addtionalOptions?.Invoke(options);
        });

        if (_registeredDbContextTypes.IsEmpty() && _applyMigrations)
        {
            services.AddHostedService(provider => new MigrationsHostedService(
                provider.GetRequiredService<IServiceScopeFactory>(),
                _registeredDbContextTypes));
        }
        _registeredDbContextTypes.Add(typeof(C));
    }

    private void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options, string schema)
    {
        var connection = provider.GetRequiredService<SqlConnection>();
        options.UseSqlServer(connection, sqlServerOptions =>
        {
            sqlServerOptions.MigrationsHistoryTable(tableName: "__EFMigrationsHistory", schema);
        });
    }
}

public static class EfCoreDataAccessExtensions
{
    public static AppBuilder AddEfCoreDataAccess<T>(
        this AppBuilder builder,
        string connectionString,
        bool applyMigrations = false,
        Action<DbContextOptionsBuilder> addtionalOptions = null) where T : DomainContext
    {
        return builder.AddDataAccess(new EfCoreDataAccess<T>(connectionString, applyMigrations, addtionalOptions));
    }
}
