using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.Commons.Collections;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Validation.DependencyInjection;

public class ValidationModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline.AddStep(typeof(ValidationStep<,>)));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;
        services.AddValidatorsFromAssemblies(app.Assemblies);

        var registrationMethod = GetType()
            .GetMethod(nameof(RegisterValidatorForValidatableObject), BindingFlags.Static | BindingFlags.NonPublic)!;

        new Commons.Reflection.AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .SubtypesOrImplementationsOf(typeof(IValidate<>))
            .NonGeneric()
            .FindTypes()
            .SelectMany(x => x.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidate<>))
            .ForEach(i => registrationMethod
                .MakeGenericMethod(i.GetGenericArguments()[0])
                .Invoke(null, new object[] { services }));
    }

    private static void RegisterValidatorForValidatableObject<T>(IServiceCollection services)
        where T : IValidate<T>
    {
        services.AddSingleton(IValidate<T>.Validator);
    }
}

public static class RequestValidationModuleExtensions
{
    public static AppBuilder AddValidation(this AppBuilder builder)
    {
        return builder.AddModule(new ValidationModule());
    }

    public static bool HasRequestValidation(this AppDescription app) => app.HasModule<ValidationModule>();
}
