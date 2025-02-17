using Autofac;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using EasyDesk.Commons.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement.DependencyInjection;

public class ErrorManagementModule : AppModule
{
    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline => pipeline
            .AddStepAfterAll(typeof(ErrorMappingStep<,>))
            .Before(typeof(DomainConstraintViolationsHandlingStep<,>)));
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        var errorTypes = new AssemblyScanner()
            .FromAssemblies(app.Assemblies)
            .SubtypesOrImplementationsOf(typeof(IMapFromDomainError<,>))
            .SubtypesOrImplementationsOf<ApplicationError>()
            .NonGeneric()
            .FindTypes();

        var mapperDictionary = CreateMapperDictionary(errorTypes);
        services.AddSingleton(_ => new GlobalErrorMapper(mapperDictionary));
    }

    private IFixedMap<Type, VersionedErrorMapper> CreateMapperDictionary(IEnumerable<Type> errorTypes)
    {
        return errorTypes
            .SelectMany(e => ExtractDomainTypes(e).Select(d => (Application: e, Domain: d)))
            .GroupBy(x => x.Domain)
            .ToFixedMap(x => x.Key, x => CreateVersionedErrorMapper(x.Key, x.Select(x => x.Application)));
    }

    private VersionedErrorMapper CreateVersionedErrorMapper(Type domain, IEnumerable<Type> application)
    {
        var versionedErrorTypes = application
            .Select(a => (Error: a, Version: a.GetApiVersionFromNamespace()))
            .ToList();

        var versionedMappers = versionedErrorTypes
            .SelectMany(x => x.Version.Map(v => (Version: v, Mapper: GetMapperForErrorPair(x.Error, domain))))
            .ToDictionary(x => x.Version, x => x.Mapper);

        var unversionedMapper = versionedErrorTypes
            .SingleOption(
                x => x.Version.IsAbsent,
                () => new InvalidOperationException($"More than one unversioned ApplicationError is mapped from {domain}"))
            .Map(x => GetMapperForErrorPair(x.Error, domain));

        return new(versionedMappers, unversionedMapper);
    }

    private Func<Error, Error> GetMapperForErrorPair(Type application, Type domain)
    {
        return (Func<Error, Error>)GetType()
            .GetMethod(nameof(GetMapperForGenericErrorPair), BindingFlags.NonPublic | BindingFlags.Instance)
            !.MakeGenericMethod(application, domain)
            .Invoke(this, null)!;
    }

    private Func<Error, Error> GetMapperForGenericErrorPair<A, D>()
        where A : ApplicationError, IMapFromDomainError<A, D>
        where D : DomainError
    {
        return error => error is D domainError ? A.MapFrom(domainError) : error;
    }

    private IEnumerable<Type> ExtractDomainTypes(Type applicationErrorType)
    {
        return applicationErrorType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFromDomainError<,>))
            .Select(i => i.GetGenericArguments())
            .Where(a => a[0] == applicationErrorType)
            .Select(a => a[1]);
    }
}

public static class ErrorManagementModuleExtensions
{
    public static IAppBuilder AddErrorManagement(this IAppBuilder builder)
    {
        return builder.AddModule(new ErrorManagementModule());
    }
}
