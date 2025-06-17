using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static class OpenApiUtils
{
    public static void ConfigureSecurityRequirement(this SwaggerGenOptions options, string name, OpenApiSecurityScheme securityScheme)
    {
        options.AddSecurityDefinition(name, securityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new()
                    {
                        Id = name,
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                Array.Empty<string>()
            },
        });
    }

    public static OpenApiSchema LookupByType(this SchemaRepository schemaRepository, Type type)
    {
        if (!schemaRepository.TryLookupByType(type, out var result))
        {
            throw new InvalidOperationException($"Schema for type {type} not found in the repository.");
        }
        return result;
    }
}
