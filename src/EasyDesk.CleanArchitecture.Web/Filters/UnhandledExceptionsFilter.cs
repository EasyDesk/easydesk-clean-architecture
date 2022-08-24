using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public class UnhandledExceptionsFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var error = ErrorDto.FromError(Errors.Internal(context.Exception));
        var response = ResponseDto<Nothing>.FromError(error);

        context.Result = new ObjectResult(response)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }
}
