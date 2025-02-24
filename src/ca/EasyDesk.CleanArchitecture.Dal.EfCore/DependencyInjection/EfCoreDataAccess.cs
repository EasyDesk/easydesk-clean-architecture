using Autofac;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auth;
using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Auditing;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;

public sealed class EfCoreDataAccess<T, TBuilder, TExtension> : IDataAccessImplementation
    where T : AbstractDbContext
    where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
    where TExtension : RelationalOptionsExtension, new()
{
    private readonly EfCoreDataAccessOptions<T, TBuilder, TExtension> _options;

    public EfCoreDataAccess(EfCoreDataAccessOptions<T, TBuilder, TExtension> options)
    {
        _options = options;
    }

    public void ConfigurePipeline(PipelineBuilder pipeline)
    {
    }

    public void AddMainDataAccessServices(ServiceRegistry registry, AppDescription app)
    {
        _options.RegisterUtilityServices(registry);
        _options.RegisterDbContext<T>(registry, isCleanArchitectureContext: false);
    }

    public void AddMessagingUtilities(ServiceRegistry registry, AppDescription app)
    {
        _options.RegisterDbContext<MessagingContext>(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreOutbox>()
                .As<IOutbox>()
                .InstancePerLifetimeScope();

            builder.RegisterType<EfCoreInbox>()
                .As<IInbox>()
                .InstancePerLifetimeScope();
        });
    }

    public void AddRolesManagement(ServiceRegistry registry, AppDescription app)
    {
        AddAuthorizationContext(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreAuthorizationManager>()
                .As<IAgentRolesProvider>()
                .As<IIdentityRolesManager>()
                .InstancePerLifetimeScope();
        });
    }

    private void AddAuthorizationContext(ServiceRegistry registry)
    {
        _options.RegisterDbContext<AuthContext>(registry);
    }

    public void AddMultitenancy(ServiceRegistry registry, AppDescription app)
    {
        AddAuthorizationContext(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreMultitenancyManager>()
                .As<IMultitenancyManager>()
                .InstancePerLifetimeScope();
        });
    }

    public void AddSagas(ServiceRegistry registry, AppDescription app)
    {
        _options.RegisterDbContext<SagasContext>(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreSagaManager>()
                .As<ISagaManager>()
                .InstancePerLifetimeScope();
        });
    }

    public void AddAuditing(ServiceRegistry registry, AppDescription app)
    {
        _options.RegisterDbContext<AuditingContext>(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreAuditLog>()
                .As<IAuditLog>()
                .InstancePerLifetimeScope();

            builder.RegisterType<EfCoreAuditStorage>()
                .As<IAuditStorageImplementation>()
                .InstancePerLifetimeScope();
        });
    }

    public void AddApiKeysManagement(ServiceRegistry registry, AppDescription app)
    {
        _options.RegisterDbContext<AuthContext>(registry);

        registry.ConfigureContainer(builder =>
        {
            builder.RegisterType<EfCoreApiKeysStorage>()
                .As<IApiKeysStorage>()
                .InstancePerLifetimeScope();
        });
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
