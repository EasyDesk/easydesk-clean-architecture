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
                    Reference = new OpenApiReference
                    {
                        Id = name,
                        Type = ReferenceType.SecurityScheme,
                    },
                },
                Array.Empty<string>()
            },
        });
    }
}
