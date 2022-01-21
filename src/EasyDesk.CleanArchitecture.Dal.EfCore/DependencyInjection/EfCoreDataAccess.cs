using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.CleanArchitecture.Dal.EfCore.Extensions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;
using EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class EfCoreDataAccess<T> : IDataAccessImplementation
    where T : EntitiesContext
{
    private readonly IConfiguration _configuration;
    private readonly bool _applyMigrations;
    private readonly Action<DbContextOptionsBuilder> _addtionalOptions;
    private readonly List<Type> _registeredDbContextTypes = new();
    private readonly Dictionary<Type, Func<RelationalTypeMapping>> _mappingsByType = new()
    {
        { typeof(Date), () => new DateMapping() },
        { typeof(Timestamp), () => new TimestampMapping() },
        { typeof(TimeOfDay), () => new TimeOfDayMapping() }
    };

    public EfCoreDataAccess(
        IConfiguration configuration,
        bool applyMigrations = false,
        Action<DbContextOptionsBuilder> addtionalOptions = null)
    {
        _configuration = configuration;
        _applyMigrations = applyMigrations;
        _addtionalOptions = addtionalOptions;
    }

    private string ConnectionString => _configuration.RequireConnectionString("MainDb");

    public void AddUtilityServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped(_ => new SqlConnection(ConnectionString));
    }

    public void AddUnitOfWork(IServiceCollection services, AppDescription app)
    {
        AddDbContext<T>(services, EntitiesContext.SchemaName, app);
        services.AddScoped<IUnitOfWork>(provider => new EfCoreUnitOfWork(provider.GetRequiredService<T>()));
    }

    public void AddTransactionManager(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<EfCoreTransactionManager>();
        services.AddScoped<ITransactionManager>(provider => provider.GetRequiredService<EfCoreTransactionManager>());
    }

    public void AddOutbox(IServiceCollection services, AppDescription app)
    {
        AddDbContext<OutboxContext>(services, OutboxContext.SchemaName, app);
        services.AddScoped<IOutbox, EfCoreOutbox>();
    }

    public void AddIdempotenceManager(IServiceCollection services, AppDescription app)
    {
        AddDbContext<IdempotenceContext>(services, IdempotenceContext.SchemaName, app);
        services.AddScoped<IIdempotenceManager, EfCoreIdempotenceManager>();
    }

    public void AddRoleBasedPermissionsProvider(IServiceCollection services, AppDescription app)
    {
        AddRoleManager(services, app);
        services.AddScoped<IPermissionsProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    public void AddRoleManager(IServiceCollection services, AppDescription app)
    {
        AddDbContext<AuthorizationContext>(services, AuthorizationContext.SchemaName, app);
        services.AddScoped<EfCoreAuthorizationManager>();
        services.AddScoped<IUserRolesProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
        services.AddScoped<IUserRolesManager>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    private void AddDbContext<C>(IServiceCollection services, string schema, AppDescription app, Action<IServiceProvider, DbContextOptionsBuilder> configure = null)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            ConfigureDbContextOptions(provider, options, schema, app);
            _addtionalOptions?.Invoke(options);
            configure?.Invoke(provider, options);
        });

        if (_registeredDbContextTypes.IsEmpty())
        {
            services.AddScoped<DbContext>(provider => provider.GetRequiredService<C>());

            if (_applyMigrations)
            {
                services.AddHostedService(provider => new MigrationsHostedService(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    _registeredDbContextTypes));
            }
        }
        _registeredDbContextTypes.Add(typeof(C));
    }

    private void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options, string schema, AppDescription app)
    {
        var connection = provider.GetRequiredService<SqlConnection>();
        options.UseSqlServer(connection, sqlServerOptions =>
        {
            ConfigureMigrationsHistoryTable(sqlServerOptions, schema);
        });
        options.AddOrUpdateExtension(new MappingPluginOptionsExtension(_mappingsByType));
        if (app.IsMultitenant())
        {
            options.AddOrUpdateExtension(new MultitenantExtension(provider.GetRequiredService<ITenantProvider>()));
        }
    }

    private void ConfigureMigrationsHistoryTable(SqlServerDbContextOptionsBuilder sqlServerOptions, string schema)
    {
        sqlServerOptions.MigrationsHistoryTable(tableName: "__EFMigrationsHistory", schema);
    }
}
