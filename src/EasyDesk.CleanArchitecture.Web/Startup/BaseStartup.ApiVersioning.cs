using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        private void AddApiVersioning(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("Api-Version"));

                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = VersioningUtils.GetSupportedVersions(WebAssemblyMarker)
                    .MaxOption()
                    .OrElseGet(() => VersioningUtils.DefaultVersion);

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
