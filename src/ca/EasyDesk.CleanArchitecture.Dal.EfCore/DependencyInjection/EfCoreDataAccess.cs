﻿using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.Authorization;
using EasyDesk.CleanArchitecture.Dal.EfCore.Domain;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.UnitOfWork;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public class EfCoreDataAccess<T, TBuilder, TExtension> : IDataAccessImplementation
    where T : DomainContext<T>
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private readonly EfCoreDataAccessOptions<TBuilder, TExtension> _options;
    private readonly ISet<Type> _registeredDbContextTypes = new HashSet<Type>();

    public EfCoreDataAccess(EfCoreDataAccessOptions<TBuilder, TExtension> options)
    {
        _options = options;
    }

    public void ConfigurePipeline(PipelineBuilder pipeline)
    {
        pipeline
            .AddStep(typeof(SaveChangesStep<,>))
            .After(typeof(UnitOfWorkStep<,>))
            .After(typeof(InboxStep<>));
    }

    public void AddMainDataAccessServices(IServiceCollection services, AppDescription app)
    {
        _options.RegisterUtilityServices(services);
        AddDbContext<T>(services);
        services.AddScoped<SaveChangesDelegate>(provider => async () =>
        {
            var dbContext = provider.GetRequiredService<T>();
            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync();
            }
        });
        services.AddScoped<EfCoreUnitOfWorkProvider>();
        services.AddScoped<IUnitOfWorkProvider>(provider => provider.GetRequiredService<EfCoreUnitOfWorkProvider>());

        services.AddScoped(provider => new MigrationsService(provider, _registeredDbContextTypes));
    }

    public void AddMessagingUtilities(IServiceCollection services, AppDescription app)
    {
        AddDbContext<MessagingContext>(
            services,
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
        AddAuthorizationContext(services);
        services.AddScoped<EfCoreAuthorizationManager>();
        services.AddScoped<IUserRolesProvider>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
        services.AddScoped<IUserRolesManager>(provider => provider.GetRequiredService<EfCoreAuthorizationManager>());
    }

    private void AddAuthorizationContext(IServiceCollection services)
    {
        AddDbContext<AuthorizationContext>(
            services,
            ConfigureMigrationsAssembly);
    }

    public void AddMultitenancy(IServiceCollection services, AppDescription app)
    {
        AddAuthorizationContext(services);
        services.AddScoped<IMultitenancyManager, EfCoreMultitenancyManager>();
    }

    public void AddSagas(IServiceCollection services, AppDescription app)
    {
        AddDbContext<SagasContext>(services, ConfigureMigrationsAssembly);
        services.AddScoped<ISagaManager, EfCoreSagaManager>();
    }

    private void ConfigureMigrationsAssembly(IServiceProvider provider, TBuilder relationalOptions)
    {
        relationalOptions.MigrationsAssembly(_options.InternalMigrationsAssembly.GetName().Name);
    }

    private void AddDbContext<C>(
        IServiceCollection services,
        Action<IServiceProvider, TBuilder>? configure = null)
        where C : DbContext
    {
        if (_registeredDbContextTypes.Contains(typeof(C)))
        {
            return;
        }

        _options.RegisterDbContext<C>(services, configure);
        _registeredDbContextTypes.Add(typeof(C));
    }
}

public static class EfCoreDataAccessExtensions
{
    public static AppBuilder AddEfCoreDataAccess<T, TBuilder, TExtension>(
        this AppBuilder builder,
        IEfCoreProvider<TBuilder, TExtension> provider,
        Action<EfCoreDataAccessOptions<TBuilder, TExtension>>? configure = null)
        where T : DomainContext<T>
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension, new()
    {
        var options = new EfCoreDataAccessOptions<TBuilder, TExtension>(provider);
        configure?.Invoke(options);
        var dataAccessImplementation = new EfCoreDataAccess<T, TBuilder, TExtension>(options);
        return builder.AddDataAccess(dataAccessImplementation);
    }

    public static async Task MigrateDatabases(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<MigrationsService>().MigrateDatabases();
    }
}