using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;
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
    private readonly string _connectionString;
    private readonly bool _applyMigrations;
    private readonly Action<DbContextOptionsBuilder> _addtionalOptions;
    private readonly List<Type> _registeredDbContextTypes = new();

    public EfCoreDataAccess(IConfiguration configuration, bool applyMigrations = false, Action<DbContextOptionsBuilder> addtionalOptions = null)
    {
        _connectionString = configuration.RequireConnectionString("MainDb");
        _applyMigrations = applyMigrations;
        _addtionalOptions = addtionalOptions;
    }

    public void AddUtilityServices(IServiceCollection services)
    {
        services.AddScoped(_ => new SqlConnection(_connectionString));
    }

    public void AddUnitOfWork(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork>(provider => new EfCoreUnitOfWork(provider.GetRequiredService<T>()));
        AddDbContext<T>(services, EntitiesContext.SchemaName);
    }

    public void AddTransactionManager(IServiceCollection services)
    {
        services.AddScoped<EfCoreTransactionManager>();
        services.AddScoped<ITransactionManager>(provider => provider.GetRequiredService<EfCoreTransactionManager>());
    }

    public void AddOutbox(IServiceCollection services)
    {
        services.AddScoped<IOutbox, EfCoreOutbox>();
        AddDbContext<OutboxContext>(services, OutboxContext.SchemaName);
    }

    public void AddIdempotenceManager(IServiceCollection services)
    {
        services.AddScoped<IIdempotenceManager, EfCoreIdempotenceManager>();
        AddDbContext<IdempotenceContext>(services, IdempotenceContext.SchemaName);
    }

    private void AddDbContext<C>(IServiceCollection services, string schema)
        where C : DbContext
    {
        services.AddDbContext<C>((provider, options) =>
        {
            ConfigureDbContextOptions(provider, options, schema);
            _addtionalOptions?.Invoke(options);
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

    private void ConfigureDbContextOptions(IServiceProvider provider, DbContextOptionsBuilder options, string schema)
    {
        var connection = provider.GetRequiredService<SqlConnection>();
        options.UseSqlServer(connection, sqlServerOptions =>
        {
            ConfigureTypeMappings(sqlServerOptions);
            ConfigureMigrationsHistoryTable(sqlServerOptions, schema);
        });
    }

    private void ConfigureTypeMappings(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        var infrastructure = sqlServerOptions as IRelationalDbContextOptionsBuilderInfrastructure;
        var builder = infrastructure.OptionsBuilder as IDbContextOptionsBuilderInfrastructure;
        var mappingsByType = new Dictionary<Type, Func<RelationalTypeMapping>>
            {
                { typeof(Date), () => new DateMapping() },
                { typeof(Timestamp), () => new TimestampMapping() },
                { typeof(TimeOfDay), () => new TimeOfDayMapping() }
            };
        builder.AddOrUpdateExtension(new MappingPluginOptionsExtension(mappingsByType));
    }

    private void ConfigureMigrationsHistoryTable(SqlServerDbContextOptionsBuilder sqlServerOptions, string schema)
    {
        sqlServerOptions.MigrationsHistoryTable(tableName: "__EFMigrationsHistory", schema);
    }
}
