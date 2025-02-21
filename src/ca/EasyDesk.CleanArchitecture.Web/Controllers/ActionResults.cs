using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public static class ActionResults
{
    public static ActionResult FromBodyAndStatusCode(object? body, HttpStatusCode statusCode)
    {
        return new ObjectResult(body)
        {
            StatusCode = (int)statusCode,
        };
    }

    public static ActionResult Forbidden(object? body) => FromBodyAndStatusCode(body, HttpStatusCode.Forbidden);

    public static ActionResult InternalServerError(object? body) => FromBodyAndStatusCode(body, HttpStatusCode.InternalServerError);

    public static ActionResult DefaultErrorHandler(this ControllerBase controller, Error error, object? body = null) => error switch
    {
        MultiError(var primary, _) => controller.DefaultErrorHandler(primary, body),
        NotFoundError => controller.NotFound(body),
        UnknownAgentError or AuthenticationFailedError => controller.Unauthorized(body),
        ForbiddenError => Forbidden(body),
        ApplicationError or DomainError => controller.BadRequest(body),
        _ => InternalServerError(body),
    };
}
