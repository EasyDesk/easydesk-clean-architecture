using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyDesk.CleanArchitecture.Web.Filters;

internal class UnhandledExceptionsFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var error = ErrorDto.FromError(Errors.Internal(context.Exception));
        var response = ResponseDto<Nothing, Nothing>.FromError(error, Nothing.Value);

        context.Result = new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}
