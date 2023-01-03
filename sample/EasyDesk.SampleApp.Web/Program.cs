using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.CleanArchitecture.Web.Controllers.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
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
        .AddAuthorization()
        .AddOpenApi()
        .AddAsyncApi()
        .AddSagas()
        .AddModule<SampleAppDomainModule>();

    config.ConfigureMultitenancy(options =>
    {
        options.DefaultPolicy = MultitenantPolicies.RequireAnyTenant();
        options.UseDefaultContextTenantReader();
    });

    config.AddPostgreSqlDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"));

    config.AddRebusMessaging("sample", (t, e) => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), e));

    config.ConfigureModule<ControllersModule>(m =>
    {
        var section = builder.Configuration.RequireSection("Pagination");
        section.GetValueAsOption<int>("DefaultPageSize").IfPresent(s => m.Options.DefaultPageSize = s);
        section.GetValueAsOption<int>("MaxPageSize").IfPresent(s => m.Options.MaxPageSize = s);
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
