using EasyDesk.Commons.Collections;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class BadRequestOperationFilter : IOperationFilter
{
    public const string ErrorCode = "4XX";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses?.ContainsKey(ErrorCode) ?? true)
        {
            return;
        }
        var successResponse = operation
            .Responses
            .FirstOption(response => response.Key.Length == 3 && response.Key.StartsWith('2'))
            .Map(kv => kv.Value);
        if (successResponse.IsAbsent)
        {
            return;
        }
        operation.Responses.Add(ErrorCode, successResponse.Value);
    }
}
