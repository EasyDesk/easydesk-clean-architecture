using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Web.Converters;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Filters;
using EasyDesk.CleanArchitecture.Web.ModelBinders;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Web.DependencyInjection
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddControllersWithPipeline(this IServiceCollection services, IWebHostEnvironment environment)
        {
            services.AddSingleton(provider => provider
                .GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>()
                .Value
                .SerializerSettings);

            services
                .AddControllers(options =>
                {
                    options.EnableEndpointRouting = false;
                    if (!environment.IsDevelopment())
                    {
                        options.Filters.Add<UnhandledExceptionsFilter>();
                    }
                    options.ModelBinderProviders.Insert(0, new TypedModelBinderProvider(new Dictionary<Type, Func<IModelBinder>>()
                    {
                        { typeof(Date), ModelBindersFactory.ForDate },
                        { typeof(Timestamp), ModelBindersFactory.ForTimestamp },
                        { typeof(Duration), ModelBindersFactory.ForDuration },
                        { typeof(TimeOfDay), ModelBindersFactory.ForTimeOfDay }
                    }));
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateParseHandling = DateParseHandling.None;

                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Date.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Timestamp.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(TimeOfDay.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(Duration.Parse));
                    options.SerializerSettings.Converters.Add(JsonConverters.FromStringParser(LocalDateTime.Parse));
                });

            return services;
        }

        public static IServiceCollection AddMediatorWithPipeline(this IServiceCollection services, params Type[] assemblyTypes)
        {
            return services
                .AddMediatR(assemblyTypes)
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviorWrapper<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviorWrapper<,>));
        }

        public static IServiceCollection AddRequestValidators(this IServiceCollection services, params Type[] assemblyTypes)
        {
            return services.AddValidatorsFromAssemblies(assemblyTypes.Select(t => t.Assembly));
        }

        public static IServiceCollection AddMappings(this IServiceCollection services, params Type[] assemblyTypes)
        {
            return services.AddAutoMapper(config =>
            {
                config.AddProfile(new DefaultMappingProfile(assemblyTypes.Append(typeof(SupportedVersionDto))));
            });
        }

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
                    if (api.ActionDescriptor is ControllerActionDescriptor descriptor)
                    {
                        return descriptor
                            .ControllerTypeInfo
                            .GetControllerVersion()
                            .Map(v => v.ToDisplayString())
                            .Contains(version);
                    }
                    return false;
                });

                config.OperationFilter<Filter>();
            });
        }

        private static void AddAuthenticationParameters(SwaggerGenOptions config)
        {
            config.AddSecurityDefinition("Auth", new OpenApiSecurityScheme
            {
                Description = "EasyDesk Authentication",
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
