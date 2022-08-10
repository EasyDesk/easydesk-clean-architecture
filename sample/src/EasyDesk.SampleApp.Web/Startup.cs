using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Web.Startup;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Authentication;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace EasyDesk.SampleApp.Web;

public class Startup : BaseStartup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment) : base(configuration, environment)
    {
    }

    protected override Type ApplicationAssemblyMarker => typeof(LogAllMessages);

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
            .AddApiVersioning()
            .AddSwagger()
            .AddModule<SampleAppDomainModule>();
            ////.AddRebusMessaging(options =>
            ////{
            ////    options
            ////        .ConfigureTransport(t => t.UseAzureServiceBusWithinEnvironment(
            ////            connectionString: Configuration.GetConnectionString("AzureServiceBus"),
            ////            inputQueueAddress: "sample-service",
            ////            environmentName: Environment.EnvironmentName))
            ////        .EnableAutoSubscribe()
            ////        .UseOutbox()
            ////        .UseIdempotentHandling();
            ////});
    }
}
