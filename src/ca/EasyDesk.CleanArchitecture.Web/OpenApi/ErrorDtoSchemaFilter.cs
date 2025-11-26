using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class ErrorDtoSchemaFilter : ISchemaFilter
{
    private readonly ISerializerDataContractResolver _serializerDataContractResolver;
    private readonly IOptions<JsonOptions> _options;
    private readonly IEnumerable<IFixedList<(Option<ApiVersion>, Type)>> _errors;

    public ErrorDtoSchemaFilter(
        ISerializerDataContractResolver serializerDataContractResolver,
        AppDescription app,
        IOptions<JsonOptions> options)
    {
        _serializerDataContractResolver = serializerDataContractResolver;
        _options = options;
        _errors = new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .FromAssembliesContaining(
                typeof(ApplicationError),
                typeof(ContextModule),
                typeof(OpenApiModule))
            .NonAbstract()
            .SubtypesOrImplementationsOf<ApplicationError>()
            .FindTypes()
            .GroupBy(e => e.Name)
            .Select(g => g.Select(e => (e.GetApiVersionFromNamespace(), e)).OrderByDescending(t => t.Item1.OrElse(new(0))).ToFixedList())
            .ToFixedList();
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        var documentVersion = OpenApiModule.DocumentNameToVersion(context.DocumentName);
        if (!type.IsAssignableTo(typeof(ErrorDto)) || schema is not OpenApiSchema concreteSchema)
        {
            return;
        }
        var errorDtoDataContract = _serializerDataContractResolver.GetDataContractForType(typeof(ErrorDto));
        var metaDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Meta));
        var codeDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Code));
        var schemaDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Schema));
        var errorMetaSchemaDictionary = _errors
            .SelectMany(list => list.FirstOption(t => MatchesVersion(t.Item1, documentVersion)).Map(t => t.Item2))
            .Select(e => (e.Name, Type: e))
            .ToSortedDictionary(
                keySelector: e => e.Name,
                elementSelector: e => GenerateSchemaForMetadataOfType(context, schema, e.Name, e.Type, codeDataProperty.Name, metaDataProperty.Name, schemaDataProperty.Name));

        errorMetaSchemaDictionary[ErrorDto.DomainErrorSchema] = GenerateSchemaForMetadataOfDomainErrors(context, schema, metaDataProperty.Name, schemaDataProperty.Name);
        errorMetaSchemaDictionary[ErrorDto.InternalErrorSchema] = GenerateSchemaForMetadataOfInternalErrors(context, schema, codeDataProperty.Name, metaDataProperty.Name, schemaDataProperty.Name);
        concreteSchema.Discriminator = new()
        {
            PropertyName = _options.Value.JsonSerializerOptions.PropertyNamingPolicy?.ConvertName(nameof(ErrorDto.Schema)) ?? nameof(ErrorDto.Schema),
            Mapping = errorMetaSchemaDictionary,
        };
        concreteSchema.OneOf = [.. errorMetaSchemaDictionary.Values,];
    }

    private static bool MatchesVersion(
        Option<ApiVersion> version,
        Option<ApiVersion> documentVersion)
    {
        if (documentVersion.IsAbsent(out var dv))
        {
            return version.IsAbsent;
        }
        return version.IsAbsent(out var v) || v <= dv;
    }

    private static IList<JsonNode> GetErrorCode(Type t) =>
        t.IsAssignableTo(typeof(ApplicationError))
        ? [ErrorDto.GetErrorCodeFromApplicationErrorType(t),]
        : [];

    private static OpenApiSchemaReference GenerateSchemaForMetadataOfType(
        SchemaFilterContext context,
        IOpenApiSchema baseSchema,
        string name,
        Type type,
        string codeDataProperty,
        string metaDataProperty,
        string schemaDataProperty)
    {
        var typeNameWithVersion = type.GetTypeNameWithVersion();
        var properties = CopyProperties(baseSchema);

        if (context.SchemaRepository.TryLookupByType(type, out var existing))
        {
            properties[metaDataProperty] = existing;
        }
        else
        {
            properties[metaDataProperty] = context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
            (properties[codeDataProperty] as OpenApiSchema)?.Enum = GetErrorCode(type);
        }
            (properties[schemaDataProperty] as OpenApiSchema)?.Enum = [name,];
        return context.SchemaRepository.AddDefinition(
            schemaId: name,
            schema: new()
            {
                Type = JsonSchemaType.Object,
                Properties = properties,
                Required = properties.Keys.ToHashSet(),
                AdditionalPropertiesAllowed = false,
                ReadOnly = true,
                Description = $"{nameof(ErrorDto)} schema generated from {typeNameWithVersion}.",
                Metadata = type.GetApiVersionFromNamespace().IsPresent(out var v)
                ? new Dictionary<string, object>
                {
                    ["version"] = v.ToStringWithoutV(),
                }
                : [],
            });
    }

    private static OpenApiSchemaReference GenerateSchemaForMetadataOfDomainErrors(
        SchemaFilterContext context,
        IOpenApiSchema baseSchema,
        string metaDataProperty,
        string schemaDataProperty)
    {
        var properties = CopyProperties(baseSchema);
        properties[metaDataProperty] = context.SchemaGenerator.GenerateSchema(typeof(object), context.SchemaRepository);
        (properties[schemaDataProperty] as OpenApiSchema)?.Enum = [ErrorDto.DomainErrorSchema,];
        return context.SchemaRepository.AddDefinition(
            schemaId: ErrorDto.DomainErrorSchema,
            schema: new()
            {
                Type = JsonSchemaType.Object,
                Properties = properties,
                Required = properties.Keys.ToHashSet(),
                AdditionalPropertiesAllowed = true,
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Object,
                },
                ReadOnly = true,
                Description = $"{nameof(ErrorDto)} schema for domain errors.",
            });
    }

    private static OpenApiSchemaReference GenerateSchemaForMetadataOfInternalErrors(
        SchemaFilterContext context,
        IOpenApiSchema baseSchema,
        string codeDataProperty,
        string metaDataProperty,
        string schemaDataProperty)
    {
        var properties = CopyProperties(baseSchema);
        properties[metaDataProperty] = context.SchemaGenerator.GenerateSchema(typeof(Nothing), context.SchemaRepository);
        (properties[codeDataProperty] as OpenApiSchema)?.Enum = [ErrorDto.InternalErrorCode,];
        (properties[schemaDataProperty] as OpenApiSchema)?.Enum = [ErrorDto.InternalErrorSchema,];
        return context.SchemaRepository.AddDefinition(
            schemaId: ErrorDto.InternalErrorSchema,
            schema: new()
            {
                Type = JsonSchemaType.Object,
                Properties = properties,
                Required = properties.Keys.ToHashSet(),
                AdditionalPropertiesAllowed = false,
                ReadOnly = true,
                Description = $"{nameof(ErrorDto)} schema for internal errors.",
            });
    }

    private static IDictionary<string, IOpenApiSchema> CopyProperties(IOpenApiSchema schema) => schema.Properties
        ?.ToSortedDictionary(schema => schema.Key, schema => schema.Value.CreateShallowCopy())
        ?? new Dictionary<string, IOpenApiSchema>();
}
