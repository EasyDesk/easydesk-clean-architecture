using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate Option<ActionResult> ErrorHandler(object body, Error error);

public delegate ActionResult SuccessHandler<T>(object body, T data)
    where T : class;

public class ActionResultBuilder<T>
    where T : class
{
    private readonly Response<T> _response;
    private readonly ControllerBase _controller;
    private readonly object _meta;
    private IImmutableList<ErrorHandler> _errorHandlers;

    public ActionResultBuilder(Response<T> response, object meta, ControllerBase controller)
        : this(response, controller, meta, List<ErrorHandler>())
    {
    }

    private ActionResultBuilder(
        Response<T> response,
        ControllerBase controller,
        object meta,
        IImmutableList<ErrorHandler> errorHandlers)
    {
        _response = response;
        _meta = meta;
        _errorHandlers = errorHandlers;
        _controller = controller;
    }

    public ActionResultBuilder<T> OnFailure(ErrorHandler errorHandler)
    {
        _errorHandlers = _errorHandlers.Add(errorHandler);
        return this;
    }

    public ActionResultBuilder<R> Map<R>(Func<T, R> mapper) where R : class
    {
        return new(_response.Map(mapper), _controller, _meta, _errorHandlers);
    }

    public ActionResult<ResponseDto<T>> OnSuccess(SuccessHandler<T> handler)
    {
        var body = ResponseDto<T>.FromResponse(_response, _meta);
        return _response.Match(
            success: t => handler(body, t),
            failure: e => HandleErrorResult(body, e));
    }

    private ActionResult HandleErrorResult(ResponseDto<T> body, Error error)
    {
        var errorToMatchAgainst = error switch
        {
            MultipleErrors(var primary, _) => primary,
            _ => error
        };
        return _errorHandlers
            .SelectMany(h => h(body, errorToMatchAgainst))
            .FirstOption()
            .OrElseGet(() => DefaultErrorHandler(body, errorToMatchAgainst));
    }

    private ActionResult DefaultErrorHandler(ResponseDto<T> body, Error error)
    {
        return error switch
        {
            NotFoundError => _controller.NotFound(body),
            DomainErrorWrapper or InputValidationError => _controller.BadRequest(body),
            ForbiddenError => ActionResults.Forbidden(body),
            UnknownUserError => _controller.Unauthorized(body),
            _ => ActionResults.InternalServerError(body)
        };
    }

    public ActionResult<ResponseDto<T>> ReturnOk() =>
        OnSuccess((body, _) => _controller.Ok(body));

    public ActionResult<ResponseDto<T>> ReturnCreatedAtAction(string actionName, Func<T, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, routeValues(result), body));

    public ActionResult<ResponseDto<T>> ReturnCreatedAtAction(string actionName, string controllerName, Func<T, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, controllerName, routeValues(result), body));
}

public static class ActionResultBuilderExtensions
{
    public static ActionResultBuilder<IEnumerable<R>> MapEachElement<T, R>(
        this ActionResultBuilder<IEnumerable<T>> builder,
        Func<T, R> mapper)
    {
        return builder.Map(ts => ts.Select(mapper));
    }
}
