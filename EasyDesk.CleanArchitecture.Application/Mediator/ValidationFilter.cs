using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.Results;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Mediator
{
    public class ValidationFilter<TRequest, TResponse> : IPipelineBehavior<TRequest, Response<TResponse>>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationFilter(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<Response<TResponse>> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<Response<TResponse>> next)
        {
            var context = new ValidationContext<TRequest>(request);
            return await _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .Select(x => Errors.InvalidProperty(x.PropertyName, x.ErrorMessage))
                .FirstOption()
                .Match(
                    some: error => Task.FromResult(Failure<TResponse>(error)),
                    none: async () => await next());
        }
    }

    // This is required since ASP.NET core DI container doesn't fully support open generics,
    // therefore the service above would not be correctly detected by MediatR.
    public class ValidationFilterWrapper<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationFilterWrapper(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var responseType = typeof(TResponse);
            var openResponseType = responseType.GetGenericTypeDefinition();

            if (!typeof(ResultBase<,>).Equals(openResponseType))
            {
                return await next();
            }

            var wrappedType = responseType.GetGenericArguments().First();
            var behaviorType = typeof(ValidationFilter<,>).MakeGenericType(typeof(TRequest), wrappedType);
            var behaviorRaw = Activator.CreateInstance(behaviorType, _validators);
            var behavior = behaviorRaw as IPipelineBehavior<TRequest, TResponse>;

            return await behavior.Handle(request, cancellationToken, next);
        }
    }
}
