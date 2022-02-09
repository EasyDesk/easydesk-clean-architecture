using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Web.Startup;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.SampleApp.Application.DomainEventHandlers;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Authentication;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.AzureServiceBus.NameFormat;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using System;

namespace EasyDesk.SampleApp.Web;

public class Startup : BaseStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment) : base(configuration, environment)
    {
    }

    protected override Type ApplicationAssemblyMarker => typeof(CoupleGotMarried);

    protected override Type InfrastructureAssemblyMarker => typeof(SampleAppContext);

    protected override Type WebAssemblyMarker => typeof(Startup);

    protected override string ServiceName => "SampleApp";

    public override void ConfigureApp(AppBuilder builder)
    {
        builder
            .AddEfCoreDataAccess<SampleAppContext>(Configuration.RequireConnectionString("MainDb"), applyMigrations: Environment.IsDevelopment())
            .AddAuthentication(options => options.AddTestAuth("Test"))
            .AddAuthorization(options => options.UseRoleBasedPermissions().WithDataAccessPermissions())
            .AddMultitenancy()
            .AddSwagger()
            .AddModule<SampleAppDomainModule>()
            .AddRebusMessaging(options =>
            {
                options
                    .ConfigureTransport(t => t.UseAzureServiceBus(Configuration.GetConnectionString("AzureServiceBus"), "sample-service"))
                    .DecorateRebusService<INameFormatter>(c => new PrefixNameFormatter("testing/", c.Get<INameFormatter>()))
                    .ConfigureRouting(r => r.TypeBased().Map<SendPersonCreatedEmail>("sample-service"))
                    .AddKnownMessageTypesFromAssembliesOf(ApplicationAssemblyMarker)
                    .UseOutbox()
                    .UseIdempotentHandling();
            });
    }
}
