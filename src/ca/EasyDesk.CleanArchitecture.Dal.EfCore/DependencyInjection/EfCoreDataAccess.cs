﻿using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auth;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Auditing;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public sealed class EfCoreDataAccess<T, TBuilder, TExtension> : IDataAccessImplementation
    where T : AbstractDbContext
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private const string MigrationsTableSuffix = "EFMigrationsHistory";
    private const string CleanArchitectureDbContextPrefix = "__CA";

    private readonly EfCoreDataAccessOptions<T, TBuilder, TExtension> _options;
    private readonly ISet<Type> _registeredDbContextTypes = new HashSet<Type>();

    public EfCoreDataAccess(EfCoreDataAccessOptions<T, TBuilder, TExtension> options)
    {
        _options = options;
    }

    public void ConfigurePipeline(PipelineBuilder pipeline)
    {
    }

    public void AddMainDataAccessServices(IServiceCollection services, AppDescription app)
    {
        _options.RegisterUtilityServices(services);
        AddDbContext<T>(services, isCleanArchitectureContext: false);
        services.AddScoped<ISaveChangesHandler, EfCoreSaveChangesHandler<T>>();
        services.AddScoped<EfCoreUnitOfWorkProvider>();
        services.AddScoped<IUnitOfWorkProvider>(provider => provider.GetRequiredService<EfCoreUnitOfWorkProvider>());

        services.AddScoped(provider => new MigrationsService(provider, _registeredDbContextTypes));

        services.AddScoped(p => MigrationCommand(p.GetRequiredService<MigrationsService>()));
    }

    private Command MigrationCommand(MigrationsService migrationsService)
    {
        var command = new Command("migrate", $"Apply migrations to the database");
        var syncOption = new Option<bool>(aliases: ["--sync", "--synchronous"], getDefaultValue: () => false, description: "Apply migrations synchronously");
        command.AddOption(syncOption);
        command.SetHandler(migrationsService.Migrate, syncOption);
        return command;
    }

    public void AddMessagingUtilities(IServiceCollection services, AppDescription app)
    {
        AddDbContext<MessagingContext>(services, ConfigureAsCleanArchitectureDbContext);
        services.AddScoped<IOutbox, EfCoreOutbox>();
        services.AddScoped<IInbox, EfCoreInbox>();
    }

    public void AddRolesManagement(IServiceCollection services, AppDescription app)
    {
        AddAuthorizationContext(services);
        services.AddScoped<EfCoreAuthorizationManager>();
        services.AddScoped<IAgentRolesProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
        services.AddScoped<IIdentityRolesManager>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    private void AddAuthorizationContext(IServiceCollection services)
    {
        AddDbContext<AuthContext>(
            services,
            ConfigureAsCleanArchitectureDbContext);
    }

    public void AddMultitenancy(IServiceCollection services, AppDescription app)
    {
        AddAuthorizationContext(services);
        services.AddScoped<IMultitenancyManager, EfCoreMultitenancyManager>();
    }

    public void AddSagas(IServiceCollection services, AppDescription app)
    {
        AddDbContext<SagasContext>(services, ConfigureAsCleanArchitectureDbContext);
        services.AddScoped<ISagaManager, EfCoreSagaManager>();
    }

    public void AddAuditing(IServiceCollection services, AppDescription app)
    {
        AddDbContext<AuditingContext>(services, ConfigureAsCleanArchitectureDbContext);
        services.AddScoped<IAuditLog, EfCoreAuditLog>();
        services.AddScoped<IAuditStorageImplementation, EfCoreAuditStorage>();
    }

    public void AddApiKeysManagement(IServiceCollection services, AppDescription app)
    {
        AddDbContext<AuthContext>(services, ConfigureAsCleanArchitectureDbContext);
        services.AddScoped<IApiKeysStorage, EfCoreApiKeysStorage>();
    }

    private void ConfigureAsCleanArchitectureDbContext(IServiceProvider provider, TBuilder relationalOptions)
    {
        relationalOptions.MigrationsAssembly(_options.InternalMigrationsAssembly.GetName().Name);
    }

    private void AddDbContext<C>(
        IServiceCollection services,
        Action<IServiceProvider, TBuilder>? configure = null,
        bool isCleanArchitectureContext = true)
        where C : DbContext
    {
        if (_registeredDbContextTypes.Contains(typeof(C)))
        {
            return;
        }

        _options.RegisterDbContext<C>(services, (provider, relationalOptions) =>
        {
            configure?.Invoke(provider, relationalOptions);

            var migrationsTableName = isCleanArchitectureContext ? $"{CleanArchitectureDbContextPrefix}_{typeof(C).Name}_{MigrationsTableSuffix}" : $"{typeof(C).Name}_{MigrationsTableSuffix}";
            relationalOptions.MigrationsHistoryTable(migrationsTableName, EfCoreUtils.MigrationsSchema);
        });
        _registeredDbContextTypes.Add(typeof(C));
    }
}

public static class EfCoreDataAccessExtensions
{
    public static IAppBuilder AddEfCoreDataAccess<T, TBuilder, TExtension>(
        this IAppBuilder builder,
        IEfCoreProvider<TBuilder, TExtension> provider,
        Action<EfCoreDataAccessOptions<T, TBuilder, TExtension>>? configure = null)
        where T : AbstractDbContext
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension, new()
    {
        var options = new EfCoreDataAccessOptions<T, TBuilder, TExtension>(provider);
        configure?.Invoke(options);
        var dataAccessImplementation = new EfCoreDataAccess<T, TBuilder, TExtension>(options);
        return builder.AddDataAccess(dataAccessImplementation);
    }
}
