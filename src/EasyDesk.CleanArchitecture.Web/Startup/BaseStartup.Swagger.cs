using EasyDesk.CleanArchitecture.Web.Swagger;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    protected abstract bool UsesSwagger { get; }

    private void AddSwagger(IServiceCollection services)
    {
        if (!UsesSwagger)
        {
            return;
        }

        services.AddSwaggerGenNewtonsoftSupport();
        services.AddSwaggerGen(options =>
        {
            VersioningUtils.GetSupportedVersions(WebAssemblyMarker)
                .Select(version => version.ToDisplayString())
                .ForEach(version =>
                {
                    options.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = ServiceName,
                        Version = version
                    });
                });

            options.OperationFilter<ApiVersionFilter>();
            options.DocInclusionPredicate((version, api) =>
            {
                if (api.ActionDescriptor.GetApiVersionModel().IsApiVersionNeutral)
                {
                    return true;
                }
                if (api.ActionDescriptor is not ControllerActionDescriptor descriptor)
                {
                    return false;
                }
                return descriptor
                    .ControllerTypeInfo
                    .GetControllerVersion()
                    .Map(v => v.ToDisplayString())
                    .Contains(version);
            });

            _schemes.ForEach(scheme => scheme.ConfigureSwagger(options));

            ConfigureSwagger(options);
        });
    }

    protected virtual void ConfigureSwagger(SwaggerGenOptions options)
    {
    }
}
