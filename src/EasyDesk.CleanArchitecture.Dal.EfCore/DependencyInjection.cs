using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.DomainEvents;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.CleanArchitecture.Dal.EfCore.Idempotence;
using EasyDesk.CleanArchitecture.Dal.EfCore.Outbox;
using EasyDesk.CleanArchitecture.Dal.EfCore.TypeMapping;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Dal.EfCore
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEfCoreDataAccess(
            this IServiceCollection services,
            string connectionString,
            Action<EfCoreDataAccessBuilder> config)
        {
            services.AddScoped(_ => new SqlConnection(connectionString));
            services.AddScoped<EfCoreTransactionManager>();
            services.AddScoped<ITransactionManager>(provider => provider.GetRequiredService<EfCoreTransactionManager>());
            services.AddScoped<IDomainEventNotifier, TransactionalDomainEventQueue>();

            var builder = new EfCoreDataAccessBuilder(services);
            config(builder);

            return services;
        }
    }

    public class EfCoreDataAccessBuilder
    {
        private readonly IServiceCollection _services;
        private bool _firstDbContextRegistered = false;

        public EfCoreDataAccessBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public EfCoreDataAccessBuilder AddEntities<T>(Action<DbContextOptionsBuilder> options = null) where T : EntitiesContext
        {
            _services.AddScoped<IUnitOfWork>(provider => new EfCoreUnitOfWork(provider.GetRequiredService<T>()));
            return AddDbContext<T>(EntitiesContext.SchemaName, options);
        }

        public EfCoreDataAccessBuilder AddOutbox(Action<DbContextOptionsBuilder> options = null)
        {
            _services.AddScoped<IOutbox, EfCoreOutbox>();
            return AddDbContext<OutboxContext>(OutboxContext.SchemaName, options);
        }

        public EfCoreDataAccessBuilder AddIdemptenceManager(Action<DbContextOptionsBuilder> options = null)
        {
            _services.AddScoped<IIdempotenceManager, EfCoreIdempotenceManager>();
            return AddDbContext<IdempotenceContext>(IdempotenceContext.SchemaName, options);
        }

        private EfCoreDataAccessBuilder AddDbContext<T>(string schema, Action<DbContextOptionsBuilder> addtionalOptions) where T : DbContext
        {
            _services.AddDbContext<T>((provider, options) =>
            {
                ConfigureDbContextOptions(provider, options, schema);
                addtionalOptions?.Invoke(options);
            });

            if (!_firstDbContextRegistered)
            {
                _services.AddScoped<DbContext>(provider => provider.GetRequiredService<T>());
                _firstDbContextRegistered = true;
            }
            return this;
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
}
