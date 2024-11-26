using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class PolymorphismSchemaFilter : ISchemaFilter
{
    private readonly IOptions<JsonOptions> _options;

    public PolymorphismSchemaFilter(IOptions<JsonOptions> options)
    {
        _options = options;
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!_options.Value.JsonSerializerOptions.TryGetTypeInfo(context.Type, out var typeInfo))
        {
            return;
        }
        if (typeInfo.PolymorphismOptions is null)
        {
            return;
        }
        schema.OneOf = typeInfo.PolymorphismOptions.DerivedTypes
            .OrderBy(x => x.TypeDiscriminator)
            .Select(x => context.SchemaGenerator.GenerateSchema(x.DerivedType, context.SchemaRepository))
            .ToList();
        schema.Discriminator = new()
        {
            PropertyName = typeInfo.PolymorphismOptions.TypeDiscriminatorPropertyName,
            Mapping = typeInfo.PolymorphismOptions.DerivedTypes.ToSortedDictionary(
                keySelector: x => (string)x.TypeDiscriminator!,
                elementSelector: x => context.SchemaRepository.LookupByType(x.DerivedType).Reference.Id),
        };
    }
}
