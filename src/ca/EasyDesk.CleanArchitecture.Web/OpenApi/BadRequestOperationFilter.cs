using EasyDesk.Commons.Collections;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.OpenApi;

internal class BadRequestOperationFilter : IOperationFilter
{
    public const string ErrorCode = "4XX";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses.ContainsKey(ErrorCode))
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
        var badRequestResponse = new OpenApiResponse(successResponse.Value)
        {
            Description = "Client Error",
        };
        operation.Responses.Add(ErrorCode, badRequestResponse);
    }
}
