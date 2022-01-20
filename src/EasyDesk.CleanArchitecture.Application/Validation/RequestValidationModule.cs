using EasyDesk.CleanArchitecture.Application.Modules;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public class RequestValidationModule : IAppModule
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddValidatorsFromAssemblyContaining(app.ApplicationAssemblyMarker);
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
