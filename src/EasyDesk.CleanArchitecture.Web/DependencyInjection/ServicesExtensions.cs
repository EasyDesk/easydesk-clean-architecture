using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, string serviceName, params Type[] controllersAssemblyTypes)
        {
            services.AddSwaggerGenNewtonsoftSupport();
            return services.AddSwaggerGen(config =>
            {
                VersioningUtils.GetSupportedVersions(controllersAssemblyTypes)
                    .Select(version => version.ToDisplayString())
                    .ForEach(version =>
                    {
                        config.SwaggerDoc(version, new OpenApiInfo
                        {
                            Title = serviceName,
                            Version = version
                        });
                    });

                AddAuthenticationParameters(config);

                config.DocInclusionPredicate((version, api) =>
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

                config.OperationFilter<Filter>();
            });
        }

        private static void AddAuthenticationParameters(SwaggerGenOptions config)
        {
            config.AddSecurityDefinition("Auth", new OpenApiSecurityScheme
            {
                Description = "Token Authentication",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http
            });
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Auth",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        }

        private class Filter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
                {
                    return;
                }
                var version = descriptor
                    .ControllerTypeInfo
                    .GetControllerVersion()
                    .IfPresent(v =>
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = "version",
                            In = ParameterLocation.Query,
                            Schema = new OpenApiSchema
                            {
                                ReadOnly = true,
                                Type = "String",
                                Default = new OpenApiString(v.ToString())
                            }
                        });
                    });
            }
        }
    }
}
