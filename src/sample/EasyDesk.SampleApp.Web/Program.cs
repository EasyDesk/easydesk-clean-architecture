using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Logging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.Dal.SqlServer;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Auditing.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Csv.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Proxy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Extensions.Configuration;
using EasyDesk.SampleApp.Application.Authorization;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Rebus.Config;
using Rebus.Timeouts;

var builder = CleanArchitectureApp.CreateBuilderWithDefaults(args);

builder
    .WithName("EasyDesk.Sample.App")
    .AddApiVersioning()
    .AddMultitenancy(options => options
        .WithDefaultPolicy(MultitenantPolicies.RequireExistingTenant()))
    .AddAuditing()
    .AddAuthentication(options => options
        .AddJwtBearer(jwt => jwt.LoadParametersFromConfiguration(builder.Configuration))
        .AddApiKey())
    .AddAuthorization(options => options.RoleBased(x => x
        .WithStaticPermissions(PermissionSettings.RolesToPermissions)))
    .AddLogging(options => options
        .EnableRequestLogging()
        .EnableResultLogging())
    .AddReverseProxy()
    .AddOpenApi()
    .AddAsyncApi()
    .AddSagas()
    .AddCsvParsing()
    .AddJsonSerialization(c =>
    {
        c.TypeInfoResolver = new TypeInfoResolver();
    })
    .AddModule<SampleAppDomainModule>();

if (builder.Environment.IsDevelopment())
{
    builder.AddModule<SampleAppDevelopmentModule>();
}

var provider = builder.Configuration.RequireValue<DbProvider>("DbProvider");
switch (provider)
{
    case DbProvider.SqlServer:
        builder.AddSqlServerDataAccess<SqlServerSampleAppContext>(() => builder.Configuration.RequireConnectionString("SqlServer"), o =>
        {
            o.WithService<SampleAppContext>();
        });
        break;
    case DbProvider.PostgreSql:
        builder.AddPostgreSqlDataAccess<PostgreSqlSampleAppContext>(() => builder.Configuration.RequireConnectionString("PostgreSql"), o =>
        {
            o.WithService<SampleAppContext>();
        });
        break;
    default:
        throw new Exception($"Invalid DB provider: {provider}");
}

builder.AddRebusMessaging(
    "sample",
    (t, e) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e.InputQueueAddress),
    options =>
    {
        options.EnableDeferredMessages(t => t.UseExternalTimeoutManager(Scheduler.Address));
        options.FailuresOptions
            .AddScheduledRetries(Retries.BackoffStrategy)
            .AddDispatchAsFailure();
    });

builder.ConfigureModule<ControllersModule>(m =>
{
    var section = builder.Configuration.GetSectionAsOption("Pagination");
    section.FlatMap(s => s.GetValueAsOption<int>("DefaultPageSize")).IfPresent(s => m.Options.DefaultPageSize = s);
    section.FlatMap(s => s.GetValueAsOption<int>("MaxPageSize")).IfPresent(s => m.Options.MaxPageSize = s);
});

builder.ConfigureWebApplication(app =>
{
    app.UseReverseProxyModule();

    app.UseHttpsRedirection();

    app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    app.UseOpenApiModule();

    app.UseAsyncApiModule();

    app.MapControllers();
});

return await builder.Run();
