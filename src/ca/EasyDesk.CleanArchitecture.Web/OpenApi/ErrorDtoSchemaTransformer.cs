using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Infrastructure.Context.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.CleanArchitecture.Web.OpenApi.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal record ErrorDto<T> where T : Error
{
    public required string Code { get; init; }

    public required string Detail { get; init; }

    public required string Schema { get; init; }

    public required T Meta { get; init; }
}

internal class ErrorDtoSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly IEnumerable<IFixedList<(Option<ApiVersion>, Type)>> _errors;

    public ErrorDtoSchemaTransformer(
        AppDescription app)
    {
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

    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var documentVersion = OpenApiModule.DocumentNameToVersion(context.DocumentName);
        if (type != typeof(ErrorDto))
        {
            return;
        }
        var metaDataProperty = context.JsonTypeInfo.Properties.First(p => p.AttributeProvider is MemberInfo { Name: nameof(ErrorDto.Meta), }).Name;
        var codeDataProperty = context.JsonTypeInfo.Properties.First(p => p.AttributeProvider is MemberInfo { Name: nameof(ErrorDto.Code), }).Name;
        var schemaDataProperty = context.JsonTypeInfo.Properties.First(p => p.AttributeProvider is MemberInfo { Name: nameof(ErrorDto.Schema), }).Name;

        var errorMetaSchemaDictionary = new Dictionary<string, IOpenApiSchema>();

        foreach (var error in _errors.SelectMany(list => list.FirstOption(t => MatchesVersion(t.Item1, documentVersion)).Map(t => t.Item2)))
        {
            await AddErrorMetaSchema(context, error.Name, errorMetaSchemaDictionary, schema, async subSchema =>
                {
                    ((OpenApiSchema)subSchema.Properties![codeDataProperty]).Const = GetErrorCode(error);
                    ((OpenApiSchema)subSchema.Properties![schemaDataProperty]).Const = error.Name;
                    if (type.GetApiVersionFromNamespace().IsPresent(out var version))
                    {
                        subSchema.Metadata ??= new Dictionary<string, object>();
                        subSchema.Metadata["version"] = version.ToStringWithoutV();
                    }
                },
                typeof(ErrorDto<>).MakeGenericType(error),
                cancellationToken);
        }

        await AddErrorMetaSchema(context, ErrorDto.DomainErrorSchema, errorMetaSchemaDictionary, schema, async subSchema =>
        {
            subSchema.Properties![metaDataProperty] = await context.GetOrCreateSchemaAsync(typeof(object), context.ParameterDescription, cancellationToken);
            ((OpenApiSchema)subSchema.Properties[schemaDataProperty]).Const = ErrorDto.DomainErrorSchema;
        },
        typeof(ErrorDto<DomainError>),
        cancellationToken);

        await AddErrorMetaSchema(context, ErrorDto.InternalErrorSchema, errorMetaSchemaDictionary, schema, async subSchema =>
        {
            subSchema.Properties![metaDataProperty] = await context.GetOrCreateSchemaAsync(typeof(Nothing), context.ParameterDescription, cancellationToken);
            ((OpenApiSchema)subSchema.Properties[codeDataProperty]).Const = ErrorDto.InternalErrorCode;
            ((OpenApiSchema)subSchema.Properties[schemaDataProperty]).Const = ErrorDto.InternalErrorSchema;
        },
        typeof(ErrorDto<InternalError>),
        cancellationToken);
        schema.Discriminator = new()
        {
            PropertyName = schemaDataProperty,
            Mapping = errorMetaSchemaDictionary.ToDictionary(x => x.Key, x => new OpenApiSchemaReference(x.Key, context.Document)),
        };
        schema.OneOf = errorMetaSchemaDictionary.Values.ToList();
        schema.Properties = null;
        schema.Required = null;
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

    private static string? GetErrorCode(Type t) =>
        t.IsAssignableTo(typeof(ApplicationError))
        ? ErrorDto.GetErrorCodeFromApplicationErrorType(t)
        : null;

    private async Task AddErrorMetaSchema(
        OpenApiSchemaTransformerContext context,
        string name,
        IDictionary<string, IOpenApiSchema> errorMetaSchemaDictionary,
        IOpenApiSchema baseSchema,
        AsyncAction<OpenApiSchema> modifySchema,
        Type targetError,
        CancellationToken cancellationToken)
    {
        var schema = await context.GetOrCreateSchemaAsync(targetError, context.ParameterDescription, cancellationToken);
        schema.AdditionalPropertiesAllowed = false;
        schema.ReadOnly = true;
        await modifySchema(schema);
        context.Document!.AddComponent(name, schema);
        errorMetaSchemaDictionary.Add(name, schema);
    }
}
