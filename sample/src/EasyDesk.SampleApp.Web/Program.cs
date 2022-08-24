using EasyDesk.CleanArchitecture.Application.Authorization.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Authorization.RoleBased.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Dal.EfCore.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Web;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.SampleApp.Infrastructure.DataAccess;
using EasyDesk.SampleApp.Web.Authentication;
using EasyDesk.SampleApp.Web.DependencyInjection;
using EasyDesk.Tools.Collections;
using Rebus.Config;

var builder = WebApplication.CreateBuilder(args);

var appDescription = builder.ConfigureForCleanArchitecture(config => config
    .AddEfCoreDataAccess<SampleAppContext>(builder.Configuration.RequireConnectionString("MainDb"), applyMigrations: builder.Environment.IsDevelopment())
    .AddAuthentication(options => options.AddTestAuth("Test"))
    .AddAuthorization(options => options.UseRoleBasedPermissions().WithDataAccessPermissions())
    .AddMultitenancy()
    .AddApiVersioning()
    .AddSwagger()
    .AddModule<SampleAppDomainModule>());
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}

if (appDescription.HasRebusMessaging())
{
    app.Services.UseRebus();
}

app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

if (appDescription.HasSwagger())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        VersioningUtils.GetSupportedVersions(appDescription)
            .Select(version => version.ToDisplayString())
            .ForEach(version =>
            {
                c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
            });
    });
}

if (appDescription.HasAuthentication())
{
    app.UseAuthentication();
}

if (appDescription.HasAspNetCoreAuthorization())
{
    app.UseAuthorization();
}

app.MapControllers();

app.Run();
