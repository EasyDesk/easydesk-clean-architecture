using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Messaging.ServiceBus.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.Startup;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.SampleApp.Application.ExternalEventHandlers;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

    protected override string ServiceName => "Sample App";

    protected override void ConfigureMvc(MvcOptions options)
    {
        // Need to add TenantTestFilter this way for testing purposes
        options.Filters.RemoveAt(options.Filters.Count - 1);
        options.Filters.Add<TenantTestFilter>();
        options.Filters.Add<TenantFilter>();
    }

    public override void ConfigureApp(AppBuilder builder)
    {
        builder
            .AddDataAccess(new EfCoreDataAccess<SampleAppContext>(Configuration, applyMigrations: Environment.IsDevelopment()))
            .AddEventManagement(new AzureServiceBus(Configuration, prefix: Environment.EnvironmentName), usesPublisher: true, usesConsumer: true)
            .AddMultitenancy()
            .AddAuthentication(options =>
            {
                options.AddScheme(new JwtBearerScheme(options =>
                {
                    options.UseJwtSettingsFromConfiguration(Configuration);
                }));
            })
            .AddAuthorization(options =>
            {
                options
                    .UseRoleBasedPermissions()
                    .WithDataAccessPermissions();
            })
            .AddSwagger();
    }
}

public enum SampleAppRole
{
    Admin,
    User
}

public enum SampleAppPermission
{
    Read,
    Write
}

public class TenantTestFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        context.HttpContext.User.AddIdentity(new ClaimsIdentity(new Claim[] { new("tenantId", "test") }));
        await next();
    }
}
