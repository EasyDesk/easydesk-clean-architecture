using Autofac;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;

internal class OpenApiGeneratorOptionsConfigurer : IConfigureNamedOptions<OpenApiOptions>
{
    private readonly AppDescription _app;
    private readonly IOptions<JsonOptions> _jsonOptions;

    public OpenApiGeneratorOptionsConfigurer(AppDescription app, IOptions<JsonOptions> jsonOptions)
    {
        _app = app;
        _jsonOptions = jsonOptions;
    }

    public void Configure(OpenApiOptions options)
    {
        SetupCommonTypes(options);
        SetupNodaTimeSupport(options);
        SetupWebSupport(options);

        _app.GetModule<OpenApiModule>().IfPresent(m =>
        {
            m.Options.ConfigureOpenApi?.Invoke(options);
        });
    }

    private void SetupCommonTypes(OpenApiOptions options)
    {
        options.AddSchemaTransformer<OptionSchemaFilter>();
        options.AddSchemaTransformer<DictionaryOfAnyKeySchemaFilter>();
        options.AddSchemaTransformer<FixedListSchemaFilter>();
        options.AddSchemaTransformer<FixedSetSchemaFilter>();
        options.AddSchemaTransformer<FixedMapSchemaFilter>();
        options.AddSchemaTransformer<CollectionsOfCustomTypesSchemaFilter>();
        options.AddSchemaTransformer<InheritanceSchemaFilter>();
        options.AddSchemaTransformer<PolymorphismSchemaFilter>();
        options.AddDocumentTransformer<PolymorphismSchemaFilter>();
        options.AddDocumentTransformer<UnusedSchemaCleaner>();
    }

    private void SetupWebSupport(OpenApiOptions options)
    {
        options.AddSchemaTransformer<ErrorDtoSchemaFilter>();
        options.AddOperationTransformer<BadRequestOperationFilter>();
        var defaultSchemaIdSelector = options.CreateSchemaReferenceId;
        options.CreateSchemaReferenceId = t =>
            t.Type.IsAssignableTo<ApplicationError>()
            ? $"{ErrorDto.GetErrorCodeFromApplicationErrorType(t.Type)}Meta"
            : defaultSchemaIdSelector(t);
    }

    private void SetupNodaTimeSupport(OpenApiOptions options)
    {
        var dateTimeZoneProvider = _app.GetModule<TimeManagementModule>()
            .Map(m => m.DateTimeZoneProvider)
            .OrElseNull();

        options.ConfigureForNodaTimeWithSystemTextJson(
            jsonSerializerOptions: _jsonOptions.Value.SerializerOptions,
            dateTimeZoneProvider: dateTimeZoneProvider);
    }

    public void Configure(string? name, OpenApiOptions options)
    {
        if (name == OpenApiModule.SingleVersionedDocumentName)
        {
            options.AddDocumentTransformer((d, _, _) =>
            {
                d.Info.Title = $"{_app.Name} API";
                d.Info.Version = name;
                return Task.CompletedTask;
            });
        }
        else if (name is not null && OpenApiModule.DocumentNameToVersion(name).IsPresent(out var version))
        {
            options.ShouldInclude = description => description.ActionDescriptor switch
            {
                ControllerActionDescriptor controllerActionDescriptor =>
                    controllerActionDescriptor.ControllerTypeInfo.GetApiVersionFromNamespace().All(v => v == version),
                var _ => false,
            };
            options
                .AddDocumentTransformer((d, _, _) =>
                {
                    d.Info.Title = $"{_app.Name} API {version}";
                    d.Info.Version = name;
                    return Task.CompletedTask;
                })
                .AddOperationTransformer(new AddApiVersionParameterFilter(version));
        }
        Configure(options);
    }
}
