using EasyDesk.CleanArchitecture.Application.Abstractions;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.AspNetCore.Mvc;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public delegate Option<ActionResult> ErrorHandler(object body, Error error);

public delegate ActionResult SuccessHandler<TResult>(object body, TResult result);

public class ActionResultBuilder<TResult, TDto, TMeta>
{
    private readonly AsyncFunc<Result<TResult>> _resultProvider;
    private readonly Func<TResult, TDto> _mapper;
    private readonly Func<Result<TResult>, Option<TMeta>> _meta;
    private readonly ControllerBase _controller;
    private IFixedList<ErrorHandler> _errorHandlers;

    public ActionResultBuilder(
        AsyncFunc<Result<TResult>> resultProvider,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, Option<TMeta>> meta,
        ControllerBase controller)
        : this(resultProvider, mapper, meta, controller, List<ErrorHandler>())
    {
    }

    private ActionResultBuilder(
        AsyncFunc<Result<TResult>> resultProvider,
        Func<TResult, TDto> mapper,
        Func<Result<TResult>, Option<TMeta>> meta,
        ControllerBase controller,
        IFixedList<ErrorHandler> errorHandlers)
    {
        _resultProvider = resultProvider;
        _mapper = mapper;
        _meta = meta;
        _errorHandlers = errorHandlers;
        _controller = controller;
    }

    public ActionResultBuilder<TResult, TDto, TMeta> OnFailure(ErrorHandler errorHandler)
    {
        _errorHandlers = _errorHandlers.Add(errorHandler);
        return this;
    }

    public ActionResultBuilder<TResult, TNewDto, TMeta> Map<TNewDto>(Func<TDto, TNewDto> mapper)
    {
        return new(_resultProvider, x => mapper(_mapper(x)), _meta, _controller, _errorHandlers);
    }

    public ActionResultBuilder<TResult, TNewDto, TMeta> MapTo<TNewDto>()
        where TNewDto : IMappableFrom<TDto, TNewDto> => Map(TNewDto.MapFrom);

    public async Task<ActionResult<ResponseDto<TDto, TMeta>>> OnSuccess(SuccessHandler<TResult> handler)
    {
        var result = await _resultProvider();
        var body = ResponseDto<TDto, TMeta>.FromResult(result.Map(_mapper), _meta(result));
        return result.Match(
            success: t => handler(body, t),
            failure: e => HandleErrorResult(body, e));
    }

    private ActionResult HandleErrorResult(ResponseDto<TDto, TMeta> body, Error error)
    {
        var errorToMatchAgainst = error switch
        {
            MultiError(var primary, _) => primary,
            _ => error,
        };
        return _errorHandlers
            .SelectMany(h => h(body, errorToMatchAgainst))
            .FirstOption()
            .OrElseGet(() => _controller.DefaultErrorHandler(errorToMatchAgainst, body));
    }

    public Task<ActionResult<ResponseDto<TDto, TMeta>>> ReturnOk() =>
        OnSuccess((body, _) => _controller.Ok(body));

    public Task<ActionResult<ResponseDto<TDto, TMeta>>> ReturnCreatedAtAction(string actionName, Func<TResult, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, routeValues(result), body));

    public Task<ActionResult<ResponseDto<TDto, TMeta>>> ReturnCreatedAtAction(string actionName, string controllerName, Func<TResult, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, controllerName, routeValues(result), body));
}

public class ActionResultBuilder<TDto, TMeta> : ActionResultBuilder<TDto, TDto, TMeta>
{
    public ActionResultBuilder(
        AsyncFunc<Result<TDto>> resultProvider,
        Func<Result<TDto>, Option<TMeta>> meta,
        ControllerBase controller)
        : base(resultProvider, It, meta, controller)
    {
    }
}
