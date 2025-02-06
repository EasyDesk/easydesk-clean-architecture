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
using Microsoft.Extensions.DependencyInjection;

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

    public void AddMainDataAccessServices(IServiceCollection services, AppDescription app)
    {
        _options.RegisterUtilityServices(services);
        _options.RegisterDbContext<T>(services, isCleanArchitectureContext: false);
    }

    public void AddMessagingUtilities(IServiceCollection services, AppDescription app)
    {
        _options.RegisterDbContext<MessagingContext>(services);
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
        _options.RegisterDbContext<AuthContext>(services);
    }

    public void AddMultitenancy(IServiceCollection services, AppDescription app)
    {
        AddAuthorizationContext(services);
        services.AddScoped<IMultitenancyManager, EfCoreMultitenancyManager>();
    }

    public void AddSagas(IServiceCollection services, AppDescription app)
    {
        _options.RegisterDbContext<SagasContext>(services);
        services.AddScoped<ISagaManager, EfCoreSagaManager>();
    }

    public void AddAuditing(IServiceCollection services, AppDescription app)
    {
        _options.RegisterDbContext<AuditingContext>(services);
        services.AddScoped<IAuditLog, EfCoreAuditLog>();
        services.AddScoped<IAuditStorageImplementation, EfCoreAuditStorage>();
    }

    public void AddApiKeysManagement(IServiceCollection services, AppDescription app)
    {
        _options.RegisterDbContext<AuthContext>(services);
        services.AddScoped<IApiKeysStorage, EfCoreApiKeysStorage>();
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
