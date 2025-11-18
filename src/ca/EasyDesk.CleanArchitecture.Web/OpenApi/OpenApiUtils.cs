using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static class OpenApiUtils
{
    public static void ConfigureSecurityRequirement(this SwaggerGenOptions options, string name, OpenApiSecurityScheme securityScheme, params List<string> scopes)
    {
        options.AddSecurityDefinition(name, securityScheme);
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(name, document)] = scopes,
        });
    }

    public static OpenApiSchemaReference LookupByType(this SchemaRepository schemaRepository, Type type)
    {
        if (!schemaRepository.TryLookupByType(type, out var result))
        {
            throw new InvalidOperationException($"Schema for type {type} not found in the repository.");
        }
        return result;
    }
}
