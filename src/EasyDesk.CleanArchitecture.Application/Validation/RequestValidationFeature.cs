using EasyDesk.CleanArchitecture.Application.Features;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Validation;

public class RequestValidationFeature : IAppFeature
{
    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddValidatorsFromAssemblyContaining(app.ApplicationAssemblyMarker);
    }
}

public static class RequestValidationFeatureExtensions
{
    public static AppBuilder AddRequestValidation(this AppBuilder builder)
    {
        return builder.AddFeature(new RequestValidationFeature());
    }

    public static bool HasRequestValidation(this AppDescription app) => app.HasFeature<RequestValidationFeature>();
}
