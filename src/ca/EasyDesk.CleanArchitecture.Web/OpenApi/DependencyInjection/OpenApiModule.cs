﻿using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using MicroElements.Swashbuckle.NodaTime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.CommandLine;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

public class OpenApiModule : AppModule
{
    private readonly OpenApiModuleOptions _options;

    public OpenApiModule(Action<OpenApiModuleOptions>? configure)
    {
        var options = new OpenApiModuleOptions();
        configure?.Invoke(options);
        _options = options;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSwaggerGenNewtonsoftSupport();
        services.AddSwaggerGen(options =>
        {
            SetupSwaggerDocs(app, options);
            SetupNodaTimeSupport(app, options);
            SetupAuthenticationSchemesSupport(app, options);
            SetupMultitenancySupport(app, options);

            _options.ConfigureSwagger?.Invoke(options);
        });
        services.Configure<SwaggerUIOptions>(c => c.DocumentTitle = $"{app.Name} - OpenAPI");
        services.AddSingleton(sp => OpenApiCommand(sp.GetRequiredService<ISwaggerProvider>(), sp.GetRequiredService<IOptions<SwaggerGenOptions>>().Value));
    }

    private void SetupMultitenancySupport(AppDescription app, SwaggerGenOptions options)
    {
        app.GetMultitenancyOptions()
            .Filter(_ => _options.AddDefaultMultitenancyFilters)
            .IfPresent(multitenancyOptions =>
            {
                if (multitenancyOptions.HttpRequestTenantReader == MultitenancyOptions.DefaultHttpRequestTenantReader)
                {
                    options.ConfigureSecurityRequirement("multitenancy", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Name = CommonTenantReaders.TenantIdHttpHeader,
                        Description = "The tenant ID to be used for the request",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "multitenancy",
                    });
                    options.OperationFilter<TenantIdOperationFilterForDefaultContextReader>();
                }
            });
    }

    private void SetupSwaggerDocs(AppDescription app, SwaggerGenOptions options)
    {
        options.SchemaFilter<OptionSchemaFilter>();
        options.SchemaFilter<ErrorCodeSchemaFilter>();
        options.SchemaFilter<NonNullableAsRequiredSchemaFilter>();
        options.OperationFilter<BadRequestOperationFilter>();
        options.SupportNonNullableReferenceTypes();
        app.GetModule<ApiVersioningModule>().Match(
            some: m => SetupApiVersionedDocs(m, app, options),
            none: () => SetupSingleVersionDoc(app, options));
    }

    private void SetupApiVersionedDocs(ApiVersioningModule module, AppDescription app, SwaggerGenOptions options)
    {
        module.ApiVersioningInfo!
            .SupportedVersions
            .Select(v => v.ToString())
            .ForEach(version => options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = $"{app.Name} {version}",
                Version = version,
            }));

        options.OperationFilter<AddApiVersionParameterFilter>();
        options.DocInclusionPredicate((version, api) =>
        {
            if (api.ActionDescriptor.GetApiVersionMetadata().IsApiVersionNeutral)
            {
                return true;
            }
            if (api.ActionDescriptor is not ControllerActionDescriptor descriptor)
            {
                return false;
            }
            return descriptor
                .ControllerTypeInfo
                .GetApiVersionFromNamespace()
                .MapToString()
                .Contains(version);
        });
    }

    private void SetupSingleVersionDoc(AppDescription app, SwaggerGenOptions options)
    {
        options.SwaggerDoc("main", new OpenApiInfo
        {
            Title = app.Name,
        });
    }

    private void SetupNodaTimeSupport(AppDescription app, SwaggerGenOptions options)
    {
        var dateTimeZoneProvider = app.GetModule<TimeManagementModule>()
            .Map(m => m.DateTimeZoneProvider)
            .OrElseNull();

        options.ConfigureForNodaTime(
            serializerSettings: JsonDefaults.DefaultSerializerSettings(),
            dateTimeZoneProvider: dateTimeZoneProvider);
    }

    private void SetupAuthenticationSchemesSupport(AppDescription app, SwaggerGenOptions options)
    {
        app.GetModule<AuthenticationModule>().IfPresent(auth =>
        {
            auth.Options.Schemes.ForEach(scheme => scheme.Value.ConfigureOpenApi(scheme.Key, options));
        });
    }

    private Command OpenApiCommand(ISwaggerProvider swaggerProvider, SwaggerGenOptions swaggerGenOptions)
    {
        var command = new Command("openapi");
        var hostOption = new Option<string?>("--host", () => null, "Sets the host value in the OpenApi document");
        var basePathOption = new Option<string?>("--base-path", () => null, "Sets the basePath value in the OpenApi document");
        var existingDocumentNames = swaggerGenOptions.SwaggerGeneratorOptions.SwaggerDocs.Keys;
        var documentNameOption = new Option<string?>(
            "--document",
            result =>
            {
                if (result.Tokens.Count == 0)
                {
                    result.ErrorMessage = "The document name is required";
                    return string.Empty;
                }
                var value = result.Tokens.Single().Value;
                if (!existingDocumentNames.Contains(value))
                {
                    result.ErrorMessage = $"The document '{value}' does not exist";
                    return string.Empty;
                }
                return value;
            },
            isDefault: true,
            description: "The name of the document to generate");
        var formatOption = new Option<OpenApiFormat>("--format", () => OpenApiFormat.Json, "The format of the output (Json or Yaml)");

        command.AddOption(documentNameOption);
        command.AddOption(hostOption);
        command.AddOption(basePathOption);
        command.AddOption(formatOption);
        command.SetHandler(
            (documentName, host, basePath, format) =>
            {
                var doc = swaggerProvider.GetSwagger(documentName, host, basePath);
                var stream = Console.OpenStandardOutput();
                doc.Serialize(stream, OpenApiSpecVersion.OpenApi3_0, format);
            },
            documentNameOption,
            hostOption,
            basePathOption,
            formatOption);

        return command;
    }
}

public static class SwaggerModuleExtensions
{
    public static IAppBuilder AddOpenApi(this IAppBuilder builder, Action<OpenApiModuleOptions>? configure = null)
    {
        return builder.AddModule(new OpenApiModule(configure));
    }

    public static bool HasOpenApi(this AppDescription app) => app.HasModule<OpenApiModule>();

    public static void UseOpenApiModule(this WebApplication app)
    {
        var swaggerOptions = app.Services.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "openapi/swagger/{documentname}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            swaggerOptions.SwaggerGeneratorOptions.SwaggerDocs.ForEach(doc =>
            {
                c.SwaggerEndpoint($"./swagger/{doc.Key}/swagger.json", doc.Value.Title);
                c.RoutePrefix = "openapi";
            });
        });
    }
}

public sealed class OpenApiModuleOptions
{
    public Action<SwaggerGenOptions>? ConfigureSwagger { get; set; }

    public bool AddDefaultMultitenancyFilters { get; set; } = true;
}
