using Autofac;
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

    protected override void ConfigureServices(AppDescription app, IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(app.Assemblies);
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;

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
                .Invoke(null, [builder,]));
    }

    private static void RegisterValidatorForValidatableObject<T>(ContainerBuilder builder)
        where T : IValidate<T>
    {
        builder.RegisterInstance(IValidate<T>.Validator)
            .As<IValidator<T>>()
            .SingleInstance();
    }
}

public static class RequestValidationModuleExtensions
{
    public static IAppBuilder AddValidation(this IAppBuilder builder)
    {
        return builder.AddModule(new ValidationModule());
    }

    public static bool HasRequestValidation(this AppDescription app) => app.HasModule<ValidationModule>();
}
