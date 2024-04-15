using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public static class ActionResults
{
    public static ActionResult FromBodyAndStatusCode(object body, HttpStatusCode statusCode)
    {
        return new ObjectResult(body)
        {
            StatusCode = (int)statusCode,
        };
    }

    public static ActionResult Forbidden(object body) => FromBodyAndStatusCode(body, HttpStatusCode.Forbidden);

    public static ActionResult InternalServerError(object body) => FromBodyAndStatusCode(body, HttpStatusCode.InternalServerError);
}
