using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

var appDescription = builder.ConfigureForCleanArchitecture(config =>
{
    config
        .WithServiceName("EasyDesk.Sample.App")
        .AddApiVersioning()
        .AddOpenApi()
        .AddAsyncApi()
        .AddModule<SampleAppDomainModule>();

    config.ConfigureMultitenancy(options =>
    {
        options.DefaultPolicy = MultitenantPolicies.RequireTenant(requireExisting: false);
        options.UseDefaultContextTenantReader();
    });

    config.AddPostgreSqlDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"));

    config.AddRebusMessaging("sample", options =>
    {
        options.ConfigureTransport((e, t) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e.InputQueueAddress));
    });
});

var app = builder.Build();

await app.MigrateDatabases();

app.UseHttpsRedirection();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseOpenApiModule();

app.UseAsyncApiModule();

app.UseAuthentication();

app.MapControllers();

app.Run();
