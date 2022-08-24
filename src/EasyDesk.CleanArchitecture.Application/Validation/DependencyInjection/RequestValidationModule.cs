using EasyDesk.CleanArchitecture.Application.Mediator.Behaviors;
using EasyDesk.CleanArchitecture.Application.Mediator.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Modules;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;

public class RequestValidationModule : AppModule
{
    private class ValidationBehaviorWrapper<TRequest, TResponse> : BehaviorWrapper<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviorWrapper(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        protected override IPipelineBehavior<TRequest, TResponse> CreateBehavior(Type requestType, Type responseType)
        {
            var behaviorType = typeof(ValidationBehavior<,>).MakeGenericType(requestType, responseType);
            return Activator.CreateInstance(behaviorType, _validators) as IPipelineBehavior<TRequest, TResponse>;
        }
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<MediatrModule>().Pipeline.AddBehavior(typeof(ValidationBehaviorWrapper<,>));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddValidatorsFromAssembly(app.GetLayerAssembly(CleanArchitectureLayer.Application));
    }
}

public static class RequestValidationModuleExtensions
{
    public static AppBuilder AddRequestValidation(this AppBuilder builder)
    {
        return builder.AddModule(new RequestValidationModule());
    }

    public static bool HasRequestValidation(this AppDescription app) => app.HasModule<RequestValidationModule>();
}
