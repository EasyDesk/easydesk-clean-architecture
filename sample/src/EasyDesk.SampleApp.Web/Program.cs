using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Authentication;
using EasyDesk.SampleApp.Web.DependencyInjection;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

var appDescription = builder.ConfigureForCleanArchitecture(config => config
    .AddEfCoreDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"), applyMigrations: true)
    .AddAuthentication(options => options.AddTestAuth("Test"))
    .AddAuthorization(options => options.UseRoleBasedPermissions().WithDataAccessPermissions())
    .AddMultitenancy()
    .AddApiVersioning()
    .AddSwagger()
    .AddRebusMessaging(options =>
    {
        options.ConfigureTransport(t => t.UseRabbitMq(builder.Configuration.RequireConnectionString("RabbitMq"), "sample"));
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

app.UseSwaggerModule();

app.UseAuthentication();

app.MapControllers();

app.Run();
