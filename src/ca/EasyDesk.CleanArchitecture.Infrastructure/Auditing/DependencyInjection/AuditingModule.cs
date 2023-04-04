using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing.DependencyInjection;

public class AuditingModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStep(typeof(AuditingStep<,>))
            .After(typeof(UnitOfWorkStep<,>))
            .After(typeof(MultitenancyManagementStep<,>)));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddAuditing(services, app);

        var channel = Channel.CreateUnbounded<(AuditRecord, TenantInfo)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        services.AddHostedService(p => new AuditingBackgroundTask(
            channel.Reader,
            p.GetRequiredService<IServiceScopeFactory>(),
            p.GetRequiredService<ILogger<AuditingBackgroundTask>>()));
        services.AddScoped<IAuditStorage>(p => new OutOfProcessAuditStorage(
            p.GetRequiredService<ITenantProvider>(),
            channel.Writer));
    }
}

public static class AuditingModuleExtensions
{
    public static AppBuilder AddAuditing(this AppBuilder builder)
    {
        return builder.AddModule(new AuditingModule());
    }
}
