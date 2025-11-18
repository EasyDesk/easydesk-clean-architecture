using Autofac;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.Versioning;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
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

    public static Commons.Options.Option<ApiVersion> DocumentNameToVersion(string documentName) => Some(documentName)
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

        // TODO: configure options to work with nodatime
    }

    private CommandLine.Command OpenApiCommand(IComponentContext context)
    {
        var command = new CommandLine.Command("openapi");
        var hostOption = new CommandLine.Option<string?>("--host")
        {
            Description = "Sets the host value in the OpenApi document",
        };
        var basePathOption = new CommandLine.Option<string?>("--base-path")
        {
            Description = "Sets the basePath value in the OpenApi document",
        };
        var existingDocumentNames = context.Resolve<IOptions<SwaggerGenOptions>>().Value.SwaggerGeneratorOptions.SwaggerDocs.Keys;
        var defaultDocumentKey = context.ResolveOption<ApiVersioningInfo>().Match(
            some: info => info.SupportedVersions.MaxOption().Map(VersionToDocumentName),
            none: () => Some(SingleVersionedDocumentName));
        var documentNameOption = new CommandLine.Option<string>("--document")
        {
            CustomParser = result =>
            {
                if (result.Tokens.Count == 0)
                {
                    if (defaultDocumentKey)
                    {
                        return defaultDocumentKey.Value;
                    }
                    result.AddError("Default document not available");
                    return string.Empty;
                }
                var value = result.Tokens.Single().Value;
                if (!existingDocumentNames.Contains(value))
                {
                    result.AddError($"The document '{value}' does not exist");
                    return string.Empty;
                }
                return value;
            },
            Required = false,
            Description = "The name of the document to generate",
        };
        var formatJsonOption = new CommandLine.Option<bool>("--json")
        {
            Description = "True if the format of the output should be Json instead of Yaml",
            DefaultValueFactory = _ => false,
        };

        command.Add(documentNameOption);
        command.Add(hostOption);
        command.Add(basePathOption);
        command.Add(formatJsonOption);
        command.SetAction(async (result, cancellationToken) =>
        {
            var doc = context.Resolve<ISwaggerProvider>().GetSwagger(result.GetValue(documentNameOption), result.GetValue(hostOption), result.GetValue(basePathOption));
            var stream = Console.OpenStandardOutput();
            await doc.SerializeAsync(stream, OpenApiSpecVersion.OpenApi3_1, result.GetValue(formatJsonOption) ? OpenApiConstants.Json : OpenApiConstants.Yaml, cancellationToken);
        });

        return command;
    }
}

public static class SwaggerModuleExtensions
{
    public const string DefaultRoutePrefix = "openapi";
    public const string DefaultEndpointTemplate = "swagger/{documentname}/swagger.json";

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
            c.RouteTemplate = $"{DefaultRoutePrefix}/{DefaultEndpointTemplate}";
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = DefaultRoutePrefix;
            swaggerOptions.GetSwaggerRelativeEndpoints().ForEach(endpoint =>
            {
                c.SwaggerEndpoint($"./{endpoint.Value}", endpoint.Key);
            });
        });
    }

    public static IFixedMap<string, string> GetSwaggerRelativeEndpoints(this SwaggerGenOptions options) => options
        .SwaggerGeneratorOptions
        .SwaggerDocs
        .ToFixedSortedMap(doc => doc.Key, doc => DefaultEndpointTemplate.Replace("{documentname}", doc.Key));

    public static IFixedMap<string, string> GetSwaggerEndpoints(this SwaggerGenOptions options) => options
        .GetSwaggerRelativeEndpoints()
        .ToFixedSortedMap(endpoint => endpoint.Key, endpoint => $"{DefaultRoutePrefix}/{endpoint.Value}");
}

public sealed class OpenApiModuleOptions
{
    public Action<SwaggerGenOptions>? ConfigureSwagger { get; set; }

    public bool AddDefaultMultitenancyFilters { get; set; } = true;
}
