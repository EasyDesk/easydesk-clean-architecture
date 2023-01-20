using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

var appDescription = builder.ConfigureForCleanArchitecture(config =>
{
    config
        .WithServiceName("EasyDesk.Sample.App")
        .AddApiVersioning()
        .AddAuthentication(configure => configure.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt => jwt.LoadParametersFromConfiguration(builder.Configuration)))
        .AddAuthorization(options => options
            .UseRoleBasedPermissions()
            .WithStaticPermissions(PermissionSettings.RolesToPermissions))
        .AddOpenApi()
        .AddAsyncApi()
        .AddSagas()
        .AddModule<SampleAppDomainModule>();

    config.ConfigureMultitenancy(options =>
    {
        options.DefaultPolicy = MultitenantPolicies.RequireExistingTenant();
        options.UseDefaultContextTenantReader();
    });

    config.AddPostgreSqlDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"));

    config.AddRebusMessaging("sample", (t, e) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e));

    config.ConfigureModule<ControllersModule>(m =>
    {
        var section = builder.Configuration.GetSectionAsOption("Pagination");
        section.FlatMap(s => s.GetValueAsOption<int>("DefaultPageSize")).IfPresent(s => m.Options.DefaultPageSize = s);
        section.FlatMap(s => s.GetValueAsOption<int>("MaxPageSize")).IfPresent(s => m.Options.MaxPageSize = s);
    });
});

var app = builder.Build();

await app.MigrateDatabases();

await app.SetupForDevelopment(async (services, logger) =>
{
    var adminId = Guid.NewGuid().ToString();
    services.GetRequiredService<IHttpContextAccessor>().SetupAuthenticatedHttpContext(adminId);
    var tenantId = TenantId.Create(Guid.NewGuid().ToString());
    await services.GetRequiredService<IMultitenancyManager>().AddTenant(tenantId);
    services.GetRequiredService<IContextTenantInitializer>().Initialize(TenantInfo.Tenant(tenantId));
    await services.GetRequiredService<IHandler<AddAdmin, Nothing>>().Handle(new AddAdmin());
    logger.LogWarning("Created tenant {tenantId} and admin with id {adminId}", tenantId, adminId);
    app.Services.LogForgedJwtForUser(adminId.ToString());
});

app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseOpenApiModule();

app.UseAsyncApiModule();

app.UseAuthentication();

app.MapControllers();

app.Run();
