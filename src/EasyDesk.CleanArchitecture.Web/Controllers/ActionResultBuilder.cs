using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Collections.ImmutableCollections;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate Option<ActionResult> ErrorHandler(object body, Error error);

public delegate ActionResult SuccessHandler<T>(object body, T data);

public class ActionResultBuilder<T>
{
    private readonly Result<T> _result;
    private readonly ControllerBase _controller;
    private readonly object _meta;
    private IImmutableList<ErrorHandler> _errorHandlers;

    public ActionResultBuilder(Result<T> result, object meta, ControllerBase controller)
        : this(result, controller, meta, List<ErrorHandler>())
    {
    }

    private ActionResultBuilder(
        Result<T> result,
        ControllerBase controller,
        object meta,
        IImmutableList<ErrorHandler> errorHandlers)
    {
        _result = result;
        _meta = meta;
        _errorHandlers = errorHandlers;
        _controller = controller;
    }

    public ActionResultBuilder<T> OnFailure(ErrorHandler errorHandler)
    {
        _errorHandlers = _errorHandlers.Add(errorHandler);
        return this;
    }

    public ActionResultBuilder<R> Map<R>(Func<T, R> mapper)
    {
        return new(_result.Map(mapper), _controller, _meta, _errorHandlers);
    }

    public ActionResult<ResponseDto<T>> OnSuccess(SuccessHandler<T> handler)
    {
        var body = ResponseDto<T>.FromResult(_result, _meta);
        return _result.Match(
            success: t => handler(body, t),
            failure: e => HandleErrorResult(body, e));
    }

    private ActionResult HandleErrorResult(ResponseDto<T> body, Error error)
    {
        var errorToMatchAgainst = error switch
        {
            MultiError(var primary, _) => primary,
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
            DomainError or InputValidationError => _controller.BadRequest(body),
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
