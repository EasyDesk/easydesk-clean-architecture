using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.Startup;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.SampleApp.Application.DomainEventHandlers;
using EasyDesk.SampleApp.Application.DomainEventHandlers.PropagatedEvents;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.AzureServiceBus.NameFormat;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

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

    protected override void ConfigureMvc(MvcOptions options)
    {
        // Need to add TenantTestFilter this way for testing purposes.
        options.Filters.RemoveAt(options.Filters.Count - 1);
        options.Filters.Add<TenantTestFilter>();
        options.Filters.Add<TenantFilter>();
    }

    public override void ConfigureApp(AppBuilder builder)
    {
        builder
            .AddDataAccess(new EfCoreDataAccess<SampleAppContext>(Configuration, applyMigrations: Environment.IsDevelopment()))
            .AddRebusMessaging(options =>
            {
                options
                    .ConfigureTransport(t => t.UseAzureServiceBus(Configuration.GetConnectionString("AzureServiceBus"), "sample-service"))
                    .DecorateRebusService<INameFormatter>(c => new PrefixNameFormatter($"testing/", c.Get<INameFormatter>()))
                    ////.ConfigureTransport(t => t.UseRabbitMq(Configuration.RequireConnectionString("RabbitMq"), "sample-service"))
                    .ConfigureRouting(r => r.TypeBased().Map<SendPersonCreatedEmail>("sample-service"))
                    .AddKnownMessageTypesFromAssembliesOf(ApplicationAssemblyMarker)
                    .UseOutbox()
                    .UseIdempotentHandling();
            })
            ////.AddAuthentication(options =>
            ////{
            ////    options.AddScheme(new JwtBearerScheme(options =>
            ////    {
            ////        options.UseJwtSettingsFromConfiguration(Configuration);
            ////    }));
            ////})
            ////.AddAuthorization(options =>
            ////{
            ////    options
            ////        .UseRoleBasedPermissions()
            ////        .WithDataAccessPermissions();
            ////})
            .AddMultitenancy()
            .AddSwagger()
            .AddModule(new SampleAppDomainModule());
    }

    public override void Configure(IApplicationBuilder app)
    {
        base.Configure(app);

        app.ApplicationServices.GetRequiredService<IBus>().Subscribe<PersonCreated>().Wait();
    }
}

public class TenantTestFilter : IAsyncActionFilter
{
    private static string _currentTenant = "test";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.HttpContext.User.AddIdentity(new ClaimsIdentity(new Claim[] { new("tenantId", _currentTenant) }));
        _currentTenant += "x";
        await next();
    }
}
