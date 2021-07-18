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
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web
{
    public static class BaseStartup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config, IWebHostEnvironment env, params Type[] assemblyTypes)
        {
            var types = assemblyTypes.Append(typeof(IServiceInstaller));
            ReflectionUtils.InstancesOfSubtypesOf<IServiceInstaller>(types.ToArray())
                .ForEach(installer => installer.InstallServices(services, config, env));
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, string serviceName, params Type[] controllersAssemblyTypes)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                VersioningUtils.GetSupportedVersions(controllersAssemblyTypes)
                    .Select(version => version.ToDisplayString())
                    .ForEach(version =>
                    {
                        c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{serviceName} {version}");
                    });
            });

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
