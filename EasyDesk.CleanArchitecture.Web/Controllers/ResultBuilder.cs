using AutoMapper;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Web.Controllers
{
    public class ResultBuilder<T>
    {
        private readonly AsyncFunc<Response<T>> _request;
        private readonly ControllerBase _controller;
        private readonly IMapper _mapper;
        private readonly List<(Predicate<Error> errorPredicate, ResultProvider<Error> resultProvider)> _errorHandlers;

        public ResultBuilder(AsyncFunc<Response<T>> request, ControllerBase controller, IMapper mapper)
        {
            _request = request;
            _controller = controller;
            _mapper = mapper;
            _errorHandlers = new List<(Predicate<Error>, ResultProvider<Error>)>();
        }

        public ResultBuilder<T> OnFailure(Predicate<Error> errorPredicate, ResultProvider<Error> resultProvider)
        {
            _errorHandlers.Add((errorPredicate, resultProvider));
            return this;
        }

        public ResponseMapper<T> OnSuccess(ResultProvider<T> resultProvider) =>
            new(map => ExecuteAction(resultProvider, map), _mapper);

        private async Task<IActionResult> ExecuteAction(ResultProvider<T> success, Func<Response<T>, object> bodyMapper)
        {
            var result = await _request();
            var body = bodyMapper(result);
            return result.Match(
                success: t => success(body, t),
                failure: e => HandleErrorResult(e, body));
        }

        private IActionResult HandleErrorResult(Error error, object body)
        {
            return _errorHandlers.FirstOption(h => h.errorPredicate(error)).Match(
                some: h => h.resultProvider(body, error),
                none: () => _controller.BadRequest(body));
        }

        public ResultBuilder<T> HandleNotFound<TNotFound>()
        {
            return OnFailure(
                e => e is NotFoundError notFoundError && notFoundError.EntityType == typeof(TNotFound),
                (body, _) => _controller.NotFound(body));
        }

        public ResponseMapper<T> ReturnOk() =>
            OnSuccess((body, _) => _controller.Ok(body));

        public ResponseMapper<T> ReturnCreatedAtAction(string actionName, Func<T, object> routeValues) =>
            OnSuccess((body, result) => _controller.CreatedAtAction(actionName, routeValues(result), body));
    }
}