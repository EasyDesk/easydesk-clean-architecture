using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public static class ActionResults
{
    public static IActionResult FromBodyAndStatusCode(object body, HttpStatusCode statusCode)
    {
        return new ObjectResult(body)
        {
            StatusCode = (int)statusCode
        };
    }

    public static IActionResult Forbidden(object body) => FromBodyAndStatusCode(body, HttpStatusCode.Forbidden);

    public static IActionResult InternalServerError(object body) => FromBodyAndStatusCode(body, HttpStatusCode.InternalServerError);
}
