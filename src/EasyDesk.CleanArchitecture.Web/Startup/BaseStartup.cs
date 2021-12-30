using System;
using System.Linq;
using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public abstract partial class BaseStartup
    {
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
            AddControllersWithPipeline(services);
            AddMediatr(services);
            AddRequestValidators(services);
            AddMappings(services);
            AddApiVersioning(services);
            AddDataAccess(services);
            AddEventManagement(services);
            AddUtilityDomainServices(services);
            AddAuthentication(services);
            AddSwagger(services);
            AddMultitenancySupport(services);

            ReflectionUtils.InstancesOfSubtypesOf<IServiceInstaller>(WebAssemblyMarker)
                .ForEach(installer => installer.InstallServices(services, Configuration, Environment));
        }

        public virtual void Configure(IApplicationBuilder app)
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

            if (UsesSwagger)
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

            if (_schemes.Any())
            {
                app.UseAuthentication();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
