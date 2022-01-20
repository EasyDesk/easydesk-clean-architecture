using System;
using System.Linq;
using EasyDesk.CleanArchitecture.Application.Events.DomainEvents;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Validation;
using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Web.Startup;

/// <summary>
/// A base class for configuring services based on the CleanArchitecture framework.
/// This implementation adds these default <see cref="IAppModule"/> to the <see cref="AppBuilder"/>:
/// <list type="bullet">
///     <item><see cref="ControllersModule"/></item>
///     <item><see cref="DomainModule"/></item>
///     <item><see cref="HttpContextModule"/></item>
///     <item><see cref="TimeManagementModule"/></item>
///     <item><see cref="MediatrModule"/></item>
///     <item><see cref="JsonSerializationModule"/></item>
///     <item><see cref="MappingModule"/></item>
///     <item><see cref="RequestValidationModule"/></item>
/// </list>
/// </summary>
public abstract class BaseStartup
{
    private AppDescription _appDescription;

    public BaseStartup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    protected IConfiguration Configuration { get; }

    protected IWebHostEnvironment Environment { get; }

    protected abstract Type ApplicationAssemblyMarker { get; }

    protected abstract Type InfrastructureAssemblyMarker { get; }

    protected abstract Type WebAssemblyMarker { get; }

    protected abstract string ServiceName { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var appBuilder = new AppBuilder()
            .AddControllers(Environment, ConfigureMvc)
            .AddDomain()
            .AddHttpContext()
            .AddTimeManagement(Configuration)
            .AddMediatr()
            .AddJsonSerialization(ConfigureJsonSettings)
            .AddMapping()
            .AddRequestValidation();

        ConfigureApp(appBuilder);

        _appDescription = appBuilder.Build(
            ServiceName,
            WebAssemblyMarker,
            ApplicationAssemblyMarker,
            InfrastructureAssemblyMarker);

        _appDescription.ConfigureServices(services);

        ReflectionUtils.InstancesOfSubtypesOf<IServiceInstaller>(WebAssemblyMarker)
            .ForEach(installer => installer.InstallServices(services, Configuration, Environment));
    }

    protected virtual void ConfigureMvc(MvcOptions options)
    {
    }

    protected virtual void ConfigureJsonSettings(JsonSerializerSettings settings)
    {
    }

    public abstract void ConfigureApp(AppBuilder builder);

    public void Configure(IApplicationBuilder app)
    {
        if (Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        if (_appDescription.HasSwagger())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                VersioningUtils.GetSupportedVersions(WebAssemblyMarker)
                    .Select(version => version.ToDisplayString())
                    .ForEach(version =>
                    {
                        c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
                    });
            });
        }

        app.UseRouting();

        if (_appDescription.HasAuthentication())
        {
            app.UseAuthentication();
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
