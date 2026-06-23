using EasyDesk.CleanArchitecture.Application.Versioning;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class AddApiVersionParameterOperationTransformer : IOpenApiOperationTransformer
{
    private readonly ApiVersion _version;

    public AddApiVersionParameterOperationTransformer(ApiVersion version)
    {
        _version = version;
    }

    private void TransformAsync(OpenApiOperation operation)
    {
        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "version",
            In = ParameterLocation.Query,
            Description = "Optional parameter used to select the API version for this endpoint.",
            Schema = new OpenApiSchema
            {
                ReadOnly = true,
                Type = JsonSchemaType.String,
                Default = _version.ToStringWithoutV(),
            },
        });
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        TransformAsync(operation);
        return Task.CompletedTask;
    }
}
