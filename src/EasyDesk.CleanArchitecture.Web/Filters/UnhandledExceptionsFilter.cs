using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public class UnhandledExceptionsFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var error = new ErrorDto(
            $"Internal server error ({context.Exception.GetType()})",
            Errors.Codes.Internal,
            Nothing.Value);

        var response = ResponseDto.FromError(error);

        context.Result = new ObjectResult(response)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }
}
