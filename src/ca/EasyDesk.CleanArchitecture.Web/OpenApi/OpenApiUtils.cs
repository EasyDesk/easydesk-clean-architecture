using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

public static partial class OpenApiUtils
{
    public static void ConfigureSecurityRequirement(this OpenApiOptions options, string name, OpenApiSecurityScheme securityScheme, params List<string> scopes)
    {
        options.AddDocumentTransformer((document, context, _) =>
        {
            document.AddComponent(name, securityScheme);
            (document.Security ??= []).Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(name, document)] = scopes,
            });
            return Task.CompletedTask;
        });
    }
}
