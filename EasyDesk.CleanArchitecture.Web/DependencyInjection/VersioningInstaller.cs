using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public class VersioningInstaller : IServiceInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("Api-Version"));

                options.AssumeDefaultVersionWhenUnspecified = false;

                options.Conventions.Add(new NamespaceConvention());
            });
        }

        private class NamespaceConvention : IControllerConvention
        {
            public bool Apply(IControllerConventionBuilder controller, ControllerModel controllerModel)
            {
                VersioningUtils.GetControllerVersion(controllerModel.ControllerType)
                    .Match(
                        some: v => controller.HasApiVersion(v),
                        none: () => controller.IsApiVersionNeutral());
                return true;
            }
        }
    }
}
