using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.Dal.SqlServer;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Auditing.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Csv.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Seeding;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Extensions.Configuration;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration.RequireValue<DbProvider>("DbProvider");
var appDescription = builder.ConfigureForCleanArchitecture(config =>
{
    config
        .WithServiceName("EasyDesk.Sample.App")
        .AddApiVersioning()
        .AddMultitenancy(options => options
            .WithDefaultPolicy(MultitenantPolicies.RequireExistingTenant()))
        .AddAuditing()
        .AddAuthentication(options => options
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwt => jwt.LoadParametersFromConfiguration(builder.Configuration)))
        .AddAuthorization(options => options
            .WithStaticPermissions(PermissionSettings.RolesToPermissions))
        .AddOpenApi()
        .AddAsyncApi()
        .AddSagas()
        .AddCsvParsing()
        .AddModule<SampleAppDomainModule>();

    switch (provider)
    {
        case DbProvider.SqlServer:
            config.AddSqlServerDataAccess<SqlServerSampleAppContext>(builder.Configuration.RequireConnectionString("SqlServer"), o =>
            {
                o.WithService<SampleAppContext>();
            });
            break;
        case DbProvider.PostgreSql:
            config.AddPostgreSqlDataAccess<PostgreSqlSampleAppContext>(builder.Configuration.RequireConnectionString("PostgreSql"), o =>
            {
                o.WithService<SampleAppContext>();
            });
            break;
        default:
            throw new Exception($"Invalid DB provider: {provider}");
    }

    config.AddRebusMessaging("sample", (t, e) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e));

    config.ConfigureModule<ControllersModule>(m =>
    {
        var section = builder.Configuration.GetSectionAsOption("Pagination");
        section.FlatMap(s => s.GetValueAsOption<int>("DefaultPageSize")).IfPresent(s => m.Options.DefaultPageSize = s);
        section.FlatMap(s => s.GetValueAsOption<int>("MaxPageSize")).IfPresent(s => m.Options.MaxPageSize = s);
    });
});

var app = builder.Build();
if (provider == DbProvider.SqlServer)
{
    // Required due to EF core bug.
    await app.MigrateSync();
}
else
{
    await app.MigrateAsync();
}

await app.SetupDevelopment(async (services, logger) =>
{
    var admin = Agent.FromSingleIdentity(Realms.MainRealm, IdentityId.FromRandomGuid());
    var tenantId = TenantId.FromRandomGuid();
    var dispatcher = services.SetupSelfScopedRequestDispatcher(admin, tenantId);
    await dispatcher.Dispatch(new CreateTenant(tenantId));
    await dispatcher.Dispatch(new AddAdmin());
    logger.LogWarning("Created tenant {tenantId} and admin with id {adminId}", tenantId, admin.MainIdentity().Id);
    services.LogForgedJwt(admin);
});

app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseOpenApiModule();

app.UseAsyncApiModule();

app.UseAuthentication();

app.MapControllers();

app.Run();
