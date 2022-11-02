using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;

public class RequestValidationModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<DispatchingModule>().Pipeline.AddStep(typeof(ValidationStep<,>));
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
