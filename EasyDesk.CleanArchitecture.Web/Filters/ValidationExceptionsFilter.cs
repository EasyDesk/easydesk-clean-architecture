using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyDesk.CleanArchitecture.Web.Filters
{
    public class ValidationExceptionsFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException validationException)
            {
                context.ExceptionHandled = true;

                var error = new ErrorDto(
                    validationException.Message,
                    "DomainValidationError",
                    Nothing.Value);

                var response = ResponseDto.FromError(error);

                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}
