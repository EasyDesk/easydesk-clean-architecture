﻿using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Web.Authentication;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EasyDesk.CleanArchitecture.Web.Filters;

public record AuthenticationFailedError(string Schema, ErrorDto InnerError) : ApplicationError
{
    public override string GetDetail() => $"Authentication with schema '{Schema}' failed.";
}

public class FailedAuthenticationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = context.HttpContext.RetrieveAuthenticationErrors();
        if (errors.IsEmpty())
        {
            await next();
            return;
        }
        context.Result = new UnauthorizedObjectResult(ResponseDto<Nothing, Nothing>.FromErrors(
            errors.Select(e => ErrorDto.FromError(new AuthenticationFailedError(
                Schema: e.Key,
                InnerError: ErrorDto.FromError(e.Value))))));
    }
}
