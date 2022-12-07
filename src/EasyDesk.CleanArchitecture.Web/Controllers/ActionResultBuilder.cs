using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate Option<ActionResult> ErrorHandler(object body, Error error);

public delegate ActionResult SuccessHandler<TResult>(object body, TResult result);

public class ActionResultBuilder<TResult, TDto>
{
    private readonly AsyncFunc<Result<TResult>> _resultProvider;
    private readonly Func<TResult, TDto> _mapper;
    private readonly Func<Result<TResult>, object> _meta;
    private readonly ControllerBase _controller;
    private IImmutableList<ErrorHandler> _errorHandlers;

    public ActionResultBuilder(
        AsyncFunc<Result<TResult>> resultProvider,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, object> meta,
        ControllerBase controller) : this(resultProvider, mapper, meta, controller, List<ErrorHandler>())
    {
    }

    private ActionResultBuilder(
        AsyncFunc<Result<TResult>> resultProvider,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, object> meta,
        ControllerBase controller,
        IImmutableList<ErrorHandler> errorHandlers)
    {
        _resultProvider = resultProvider;
        _mapper = mapper;
        _meta = meta;
        _errorHandlers = errorHandlers;
        _controller = controller;
    }

    public ActionResultBuilder<TResult, TDto> OnFailure(ErrorHandler errorHandler)
    {
        _errorHandlers = _errorHandlers.Add(errorHandler);
        return this;
    }

    public ActionResultBuilder<TResult, TNewDto> Map<TNewDto>(Func<TDto, TNewDto> mapper)
    {
        return new(_resultProvider, x => mapper(_mapper(x)), _meta, _controller, _errorHandlers);
    }

    public async Task<ActionResult<ResponseDto<TDto>>> OnSuccess(SuccessHandler<TResult> handler)
    {
        var result = await _resultProvider();
        var body = ResponseDto<TDto>.FromResult(result.Map(_mapper), _meta(result));
        return result.Match(
            success: t => handler(body, t),
            failure: e => HandleErrorResult(body, e));
    }

    private ActionResult HandleErrorResult(ResponseDto<TDto> body, Error error)
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

    private ActionResult DefaultErrorHandler(ResponseDto<TDto> body, Error error)
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

    public Task<ActionResult<ResponseDto<TDto>>> ReturnOk() =>
        OnSuccess((body, _) => _controller.Ok(body));

    public Task<ActionResult<ResponseDto<TDto>>> ReturnCreatedAtAction(string actionName, Func<TResult, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, routeValues(result), body));

    public Task<ActionResult<ResponseDto<TDto>>> ReturnCreatedAtAction(string actionName, string controllerName, Func<TResult, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, controllerName, routeValues(result), body));
}

public static class ActionResultBuilderExtensions
{
    public static ActionResultBuilder<PageInfo<TResult>, IEnumerable<TNewDto>> MapEachElement<TResult, TDto, TNewDto>(
        this ActionResultBuilder<PageInfo<TResult>, IEnumerable<TDto>> builder,
        Func<TDto, TNewDto> mapper)
    {
        return builder.Map(ts => ts.Select(mapper));
    }
}
