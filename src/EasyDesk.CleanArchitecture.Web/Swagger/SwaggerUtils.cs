using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Swagger;

public static class SwaggerUtils
{
    public static void ConfigureJwtBearerAuthentication(this SwaggerGenOptions options, string name = "Bearer")
    {
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Description = "Token Authentication",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        };
        options.ConfigureSecurityRequirement(name, jwtSecurityScheme);
    }

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
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
    }
}
