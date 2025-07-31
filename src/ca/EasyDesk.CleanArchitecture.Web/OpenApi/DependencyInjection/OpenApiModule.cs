using Autofac;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Extensions.DependencyInjection;
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
using static System.CommandLine.Handler;
using CommandLine = System.CommandLine;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

public class OpenApiModule : AppModule
{
    public const string SingleVersionedDocumentName = "main";

    private readonly OpenApiModuleOptions _options;

    public OpenApiModule(Action<OpenApiModuleOptions>? configure)
    {
        var options = new OpenApiModuleOptions();
        configure?.Invoke(options);
        _options = options;
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder
            .Register(c => OpenApiCommand(c.Resolve<IComponentContext>()))
            .SingleInstance();
    }

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            SetupSwaggerDocs(app, options);
            SetupNodaTimeSupport(app, options);
            SetupMultitenancySupport(app, options);
            var defaultSchemaIdSelector = options.SchemaGeneratorOptions.SchemaIdSelector;
            options.CustomSchemaIds(t => t.IsAssignableTo<ApplicationError>() ? $"{ErrorDto.GetErrorCodeFromApplicationErrorType(t)}Meta" : defaultSchemaIdSelector(t));
            _options.ConfigureSwagger?.Invoke(options);
        });
        services.Configure<SwaggerUIOptions>(c => c.DocumentTitle = $"{app.Name} - OpenAPI");
    }

    private void SetupMultitenancySupport(AppDescription app, SwaggerGenOptions options)
    {
        app.GetMultitenancyOptions()
            .Filter(_ => _options.AddDefaultMultitenancyFilters)
            .IfPresent(multitenancyOptions =>
            {
                if (multitenancyOptions.HttpRequestTenantReader != MultitenancyOptions.DefaultHttpRequestTenantReader)
                {
                    return;
                }

                options.ConfigureSecurityRequirement("multitenancy", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = CommonTenantReaders.TenantIdHttpHeader,
                    Description = "The tenant ID to be used for the request",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "multitenancy",
                });
                options.OperationFilter<TenantIdOperationFilterForDefaultContextReader>();
            });
    }

    private void SetupSwaggerDocs(AppDescription app, SwaggerGenOptions options)
    {
        options.SchemaFilter<OptionSchemaFilter>();
        options.SchemaFilter<ErrorDtoSchemaFilter>();
        options.SchemaFilter<PolymorphismSchemaFilter>();
        options.OperationFilter<BadRequestOperationFilter>();
        options.SchemaFilter<FixedMapSchemaFilter>();
        options.SupportNonNullableReferenceTypes();
        app.GetModule<ApiVersioningModule>().Match(
            some: m => SetupApiVersionedDocs(m, app, options),
            none: () => SetupSingleVersionDoc(app, options));
    }

    private void SetupApiVersionedDocs(ApiVersioningModule module, AppDescription app, SwaggerGenOptions options)
    {
        module.ApiVersioningInfo!
            .SupportedVersions
            .Select(VersionToDocumentName)
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

    public static string VersionToDocumentName(ApiVersion v) => v.ToString();

    public static Option<ApiVersion> DocumentNameToVersion(string documentName) => Some(documentName)
        .Filter(d => d != SingleVersionedDocumentName)
        .Map(d => ApiVersion.Parse(d).OrElseThrow(() => throw new InvalidOperationException("Unable to retrieve the Api version from the document name.")));

    private void SetupSingleVersionDoc(AppDescription app, SwaggerGenOptions options)
    {
        options.SwaggerDoc(SingleVersionedDocumentName, new OpenApiInfo
        {
            Title = app.Name,
        });
    }

    private void SetupNodaTimeSupport(AppDescription app, SwaggerGenOptions options)
    {
        var dateTimeZoneProvider = app.GetModule<TimeManagementModule>()
            .Map(m => m.DateTimeZoneProvider)
            .OrElseNull();

        options.ConfigureForNodaTimeWithSystemTextJson(
            jsonSerializerOptions: JsonDefaults.DefaultSerializerOptions(),
            dateTimeZoneProvider: dateTimeZoneProvider);
    }

    private CommandLine.Command OpenApiCommand(IComponentContext context)
    {
        var command = new CommandLine.Command("openapi");
        var hostOption = new CommandLine.Option<string?>("--host", () => null, "Sets the host value in the OpenApi document");
        var basePathOption = new CommandLine.Option<string?>("--base-path", () => null, "Sets the basePath value in the OpenApi document");
        var existingDocumentNames = context.Resolve<IOptions<SwaggerGenOptions>>().Value.SwaggerGeneratorOptions.SwaggerDocs.Keys;
        var defaultDocumentKey = context.ResolveOption<ApiVersioningInfo>().Match(
            some: info => info.SupportedVersions.MaxOption().Map(VersionToDocumentName),
            none: () => Some(SingleVersionedDocumentName));
        var documentNameOption = new CommandLine.Option<string>(
            "--document",
            result =>
            {
                if (result.Tokens.Count == 0)
                {
                    if (defaultDocumentKey)
                    {
                        return defaultDocumentKey.Value;
                    }
                    result.ErrorMessage = "Default document not available";
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
        var formatOption = new CommandLine.Option<OpenApiFormat>("--format", () => OpenApiFormat.Json, "The format of the output (Json or Yaml)");

        command.AddOption(documentNameOption);
        command.AddOption(hostOption);
        command.AddOption(basePathOption);
        command.AddOption(formatOption);
        command.SetHandler(
            (documentName, host, basePath, format) =>
            {
                var doc = context.Resolve<ISwaggerProvider>().GetSwagger(documentName, host, basePath);
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
