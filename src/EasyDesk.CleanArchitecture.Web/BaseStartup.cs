using EasyDesk.CleanArchitecture.Web.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Collections.EnumerableUtils;

namespace EasyDesk.CleanArchitecture.Web
{
    public abstract class BaseStartup
    {
        public BaseStartup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        protected IConfiguration Configuration { get; }

        protected IWebHostEnvironment Environment { get; }

        protected virtual IEnumerable<Type> ServiceInstallersAssemblyMarkers => Items(GetType(), typeof(IServiceInstaller));

        protected virtual IEnumerable<Type> ControllersAssemblyMarkers => Items(GetType());

        protected abstract bool UseAuthentication { get; }

        protected abstract bool UseAuthorization { get; }

        protected abstract bool UseSwagger { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            ReflectionUtils.InstancesOfSubtypesOf<IServiceInstaller>(ServiceInstallersAssemblyMarkers)
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

            if (UseSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    VersioningUtils.GetSupportedVersions(ControllersAssemblyMarkers)
                        .Select(version => version.ToDisplayString())
                        .ForEach(version =>
                        {
                            c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
                        });
                });
            }

            app.UseRouting();

            if (UseAuthentication)
            {
                app.UseAuthentication();
            }

            if (UseAuthorization)
            {
                app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
