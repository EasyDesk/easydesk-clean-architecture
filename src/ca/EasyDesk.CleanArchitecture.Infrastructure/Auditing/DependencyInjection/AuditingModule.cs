using Autofac;
using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.Hosting;
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
            .After(typeof(MultitenancyManagementStep<,>))
            .After(typeof(AuthenticationStep<,>)));
    }

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        app.RequireModule<DataAccessModule>().Implementation.AddAuditing(registry, app);

        registry.ConfigureContainer(builder =>
        {
            var channel = Channel.CreateUnbounded<(AuditRecord, TenantInfo)>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
            });

            builder
                .Register(c => new AuditingBackgroundTask(
                    c.Resolve<ILifetimeScope>(),
                    channel.Reader,
                    c.Resolve<ILogger<AuditingBackgroundTask>>()))
                .As<IHostedService>()
                .SingleInstance();

            builder
                .Register(c => new OutOfProcessAuditStorage(
                    c.Resolve<ITenantProvider>(),
                    channel.Writer))
                .As<IAuditStorage>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<AuditConfigurer>()
                .As<IAuditConfigurer>()
                .InstancePerLifetimeScope();
        });
    }
}

public static class AuditingModuleExtensions
{
    public static IAppBuilder AddAuditing(this IAppBuilder builder)
    {
        return builder.AddModule(new AuditingModule());
    }
}
