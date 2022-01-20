using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Pages;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Controllers;

public class ActionResultBuilder<T>
{
    private readonly AsyncFunc<Response<T>> _request;
    private readonly ControllerBase _controller;
    private readonly List<(Predicate<Error> ErrorPredicate, ActionResultProvider<Error> ResultProvider)> _errorHandlers = new();
    private Func<T, ResponseDto> _successfulResponseConverter = x => ResponseDto.FromData(x);

    public ActionResultBuilder(AsyncFunc<Response<T>> request, ControllerBase controller)
    {
        _request = request;
        _controller = controller;
    }

    public ActionResultBuilder<T> ConvertSuccessfulResponseWith(Func<T, ResponseDto> converter)
    {
        _successfulResponseConverter = converter;
        return this;
    }

    public ActionResultBuilder<T> OnFailure(Predicate<Error> errorPredicate, ActionResultProvider<Error> resultProvider)
    {
        _errorHandlers.Add((errorPredicate, resultProvider));
        return this;
    }

    public async Task<IActionResult> OnSuccess(ActionResultProvider<T> resultProvider)
    {
        var response = await _request();
        var body = response.Match(
            success: t => _successfulResponseConverter(t),
            failure: e => CreateErrorResponse(e));
        return response.Match(
            success: t => resultProvider(body, t),
            failure: e => HandleErrorResult(e, body));
    }

    private ResponseDto CreateErrorResponse(Error error) =>
       ResponseDto.FromErrors(error switch
       {
           MultipleErrors(var errors) => errors.Select(ErrorDto.FromError),
           _ => Some(ErrorDto.FromError(error))
       });

    private IActionResult HandleErrorResult(Error error, ResponseDto body)
    {
        return _errorHandlers.FirstOption(h => h.ErrorPredicate(error)).Match(
            some: h => h.ResultProvider(body, error),
            none: () => DefaultErrorHandler(error, body));
    }

    private IActionResult DefaultErrorHandler(Error error, object body) => error switch
    {
        MultipleErrors(var errors) => DefaultErrorHandler(errors[0], body),
        NotFoundError => _controller.NotFound(body),
        DomainErrorWrapper => _controller.BadRequest(body),
        ForbiddenError => ActionResults.Forbidden(body),
        UnknownUserError => _controller.Unauthorized(body),
        _ => ActionResults.InternalServerError(body)
    };

    public Task<IActionResult> ReturnOk() =>
        OnSuccess((body, _) => _controller.Ok(body));

    public Task<IActionResult> ReturnCreatedAtAction(string actionName, Func<T, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, routeValues(result), body));

    public Task<IActionResult> ReturnCreatedAtAction(string actionName, string controllerName, Func<T, object> routeValues) =>
        OnSuccess((body, result) => _controller.CreatedAtAction(actionName, controllerName, routeValues(result), body));
}

public static class ActionResultBuilderExtensions
{
    public static ActionResultBuilder<T> MappingContent<T>(this ActionResultBuilder<T> builder, Func<T, object> mapper)
    {
        return builder.ConvertSuccessfulResponseWith(value => ResponseDto.FromData(mapper(value)));
    }

    public static ActionResultBuilder<Page<T>> Paging<T>(this ActionResultBuilder<Page<T>> builder, Func<T, object> mapper = null)
    {
        return builder.ConvertSuccessfulResponseWith(page =>
        {
            var meta = new PaginationResponseMetaDto(
                page.Pagination.PageIndex,
                page.Pagination.PageSize,
                page.Count,
                page.PageCount);
            return ResponseDto.FromData(mapper is null ? page.Items : page.Items.Select(mapper), meta);
        });
    }
}
