using Autofac;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.OpenApi.NodaTime;
using EasyDesk.CleanArchitecture.Web.Versioning.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

internal class SwaggerGenOptionsConfigurer : IConfigureOptions<SwaggerGenOptions>
{
    private readonly AppDescription _app;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public SwaggerGenOptionsConfigurer(AppDescription app, IOptions<JsonOptions> jsonOptions)
    {
        _app = app;
        _jsonOptions = jsonOptions;
    }

    public void Configure(SwaggerGenOptions options)
    {
        SetupCommonTypes(options);
        SetupNodaTimeSupport(options);
        SetupWebSupport(options);

        _app.GetModule<ApiVersioningModule>().Match(
            some: m => SetupApiVersionedDocs(m, _app, options),
            none: () => SetupSingleVersionDoc(_app, options));

        _app.GetModule<OpenApiModule>().IfPresent(m =>
        {
            SetupMultitenancySupport(m, options);
            m.Options.ConfigureSwagger?.Invoke(options);
        });
    }

    private void SetupCommonTypes(SwaggerGenOptions options)
    {
        options.SupportNonNullableReferenceTypes();
        options.SchemaFilter<OptionSchemaFilter>();
        options.SchemaFilter<PolymorphismSchemaFilter>();
        options.DocumentFilter<UnusedSchemaCleaner>();
        options.MapType<IFixedMap<string, object>>(() => new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
            },
        });
    }

    private void SetupWebSupport(SwaggerGenOptions options)
    {
        options.SchemaFilter<ErrorDtoSchemaFilter>();
        options.OperationFilter<BadRequestOperationFilter>();
        var defaultSchemaIdSelector = options.SchemaGeneratorOptions.SchemaIdSelector;
        options.CustomSchemaIds(t =>
            t.IsAssignableTo<ApplicationError>()
            ? $"{ErrorDto.GetErrorCodeFromApplicationErrorType(t)}Meta"
            : defaultSchemaIdSelector(t));
    }

    private void SetupNodaTimeSupport(SwaggerGenOptions options)
    {
        var dateTimeZoneProvider = _app.GetModule<TimeManagementModule>()
            .Map(m => m.DateTimeZoneProvider)
            .OrElseNull();

        options.ConfigureForNodaTimeWithSystemTextJson(
            jsonSerializerOptions: _jsonOptions.Value.SerializerOptions,
            dateTimeZoneProvider: dateTimeZoneProvider);
    }

    private void SetupMultitenancySupport(OpenApiModule module, SwaggerGenOptions options)
    {
        _app.GetMultitenancyOptions()
            .Filter(_ => module.Options.AddDefaultMultitenancyFilters)
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

    private void SetupApiVersionedDocs(ApiVersioningModule module, AppDescription app, SwaggerGenOptions options)
    {
        module.ApiVersioningInfo!
            .SupportedVersions
            .Select(OpenApiModule.VersionToDocumentName)
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
        options.SwaggerDoc(OpenApiModule.SingleVersionedDocumentName, new OpenApiInfo
        {
            Title = app.Name,
        });
    }
}
