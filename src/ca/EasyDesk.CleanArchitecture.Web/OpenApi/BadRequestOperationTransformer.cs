using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class BadRequestOperationTransformer : IOpenApiOperationTransformer
{
    public const string ErrorCode = "4XX";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Responses?.ContainsKey(ErrorCode) ?? true)
        {
            return Task.CompletedTask;
        }
        var successResponse = operation
            .Responses
            .FirstOption(response => response.Key.Length == 3 && response.Key.StartsWith('2'))
            .Map(kv => kv.Value);
        if (successResponse.IsAbsent)
        {
            return Task.CompletedTask;
        }
        operation.Responses.Add(ErrorCode, successResponse.Value);
        return Task.CompletedTask;
    }
}
