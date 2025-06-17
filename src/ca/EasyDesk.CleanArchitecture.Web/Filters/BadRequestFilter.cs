using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyDesk.CleanArchitecture.Web.Filters;

internal class BadRequestFilter : IResultFilter
{
    private const string ErrorCode = "InvalidValue";

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not BadRequestObjectResult badRequestObjectResult
            || badRequestObjectResult.Value is not ValidationProblemDetails errorsSource)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(
            ResponseDto<Nothing, Nothing>.FromErrors(
                errorsSource.Errors
                    .Select(errorSource =>
                        Errors.InvalidInput(
                            errorSource.Key,
                            ErrorCode,
                            errorSource.Value.ConcatStrings("\n")))
                    .Select(ErrorDto.FromError)));
    }
}
