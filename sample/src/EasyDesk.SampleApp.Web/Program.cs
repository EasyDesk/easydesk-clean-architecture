using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.PostgreSql;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.AsyncApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Authentication;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);

var appDescription = builder.ConfigureForCleanArchitecture(config => config
    .AddPostgreSqlDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"), options =>
    {
        options.ApplyMigrations();
    })
    .WithServiceName("EasyDesk.Sample.App")
    .AddAuthentication(options => options.AddTestAuth("Test"))
    .AddAuthorization(options => options.UseRoleBasedPermissions().WithDataAccessPermissions())
    .AddMultitenancy()
    .AddApiVersioning()
    .AddOpenApi()
    .AddAsyncApi()
    .AddRebusMessaging("sample", options =>
    {
        options.ConfigureTransport(t => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), options.InputQueueAddress));
        options.ConfigureRouting(t => t.TypeBased().Map<WelcomePerson>(options.InputQueueAddress));
    })
    .AddModule<SampleAppDomainModule>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

app.Services.UseRebus();

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseOpenApiModule();

app.UseAsyncApiModule();

app.UseAuthentication();

app.MapControllers();

app.Run();
