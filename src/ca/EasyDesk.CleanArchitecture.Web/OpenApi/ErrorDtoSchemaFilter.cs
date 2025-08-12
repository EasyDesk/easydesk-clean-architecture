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
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var type = context.Type;
        var documentVersion = OpenApiModule.DocumentNameToVersion(context.DocumentName);
        if (!type.IsAssignableTo(typeof(ErrorDto)))
        {
            return;
        }
        var errorDtoDataContract = _serializerDataContractResolver.GetDataContractForType(typeof(ErrorDto));
        var metaDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Meta));
        var codeDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Code));
        var schemaDataProperty = errorDtoDataContract.ObjectProperties.First(p => p.MemberInfo.Name == nameof(ErrorDto.Schema));
        var errorMetaSchemaDictionary = _errors
            .SelectMany(list => list.FirstOption(t => MatchesVersion(t.Item1, documentVersion)).Map(t => t.Item2))
            .Select(e => (e.Name, e))
            .Append((ErrorDto.DomainErrorSchema, typeof(Nothing)))
            .Append((ErrorDto.InternalErrorSchema, typeof(Nothing)))
            .ToSortedDictionary(
                keySelector: e => e.Item1,
                elementSelector: e =>
                {
                    var properties = schema.Properties.ToSortedDictionary(schema => schema.Key, schema => new OpenApiSchema(schema.Value));
                    if (context.SchemaRepository.TryLookupByType(e.Item2, out var existing))
                    {
                        properties[metaDataProperty.Name] = existing;
                    }
                    else
                    {
                        var schema = context.SchemaGenerator.GenerateSchema(e.Item2, context.SchemaRepository);
                        properties[codeDataProperty.Name].Enum = GetErrorCode(e.Item2);
                        properties[metaDataProperty.Name] = schema;
                    }
                    properties[schemaDataProperty.Name].Enum = [new OpenApiString(e.Item1),];
                    return context.SchemaRepository.AddDefinition(
                        schemaId: e.Item1,
                        schema: new()
                        {
                            Type = "object",
                            Properties = properties,
                            Required = properties.Keys.ToHashSet(),
                            AdditionalPropertiesAllowed = false,
                            ReadOnly = true,
                            Description = $"{nameof(ErrorDto)} schema generated from {e.Item2.GetTypeNameWithVersion()}.",
                            Annotations = e.Item2.GetApiVersionFromNamespace().IsPresent(out var v)
                            ? new Dictionary<string, object>
                            {
                                ["version"] = v.ToStringWithoutV(),
                            }
                            : [],
                        });
                });
        schema.Discriminator = new()
        {
            PropertyName = _options.Value.JsonSerializerOptions.PropertyNamingPolicy?.ConvertName(nameof(ErrorDto.Schema)) ?? nameof(ErrorDto.Schema),
            Mapping = errorMetaSchemaDictionary.ToSortedDictionary(entry => entry.Key, entry => entry.Value.Reference.Id),
        };
        schema.OneOf = errorMetaSchemaDictionary.Values.ToList();
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

    private IList<IOpenApiAny> GetErrorCode(Type t) =>
        t.IsAssignableTo(typeof(ApplicationError))
        ? [new OpenApiString(ErrorDto.GetErrorCodeFromApplicationErrorType(t)),]
        : [];
}
