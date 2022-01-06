using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public class DomainConstraintsFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DomainConstraintException validationException)
        {
            context.ExceptionHandled = true;

            var error = new ErrorDto(
                validationException.Message,
                "DomainConstraintViolated",
                Nothing.Value);

            var response = ResponseDto.FromError(error);

            context.Result = new BadRequestObjectResult(response);
        }
    }
}
