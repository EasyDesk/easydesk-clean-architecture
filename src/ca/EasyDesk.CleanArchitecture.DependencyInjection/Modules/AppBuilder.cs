using EasyDesk.Commons.Collections.Immutable;
using System.Reflection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class AppBuilder
{
    private readonly ModulesCollection _modules = new();
    private IFixedSet<Assembly> _assemblies = Set<Assembly>();
    private string _serviceName;

    public AppBuilder(string serviceName)
    {
        _serviceName = serviceName;
    }

    public AppBuilder WithServiceName(string name)
    {
        _serviceName = name;
        return this;
    }

    public AppBuilder WithAssemblies(params Assembly[] assemblies) =>
        WithAssemblies(assemblies.AsEnumerable());

    public AppBuilder WithAssemblies(IEnumerable<Assembly> assemblies)
    {
        _assemblies = _assemblies.Union(assemblies);
        return this;
    }

    public AppBuilder AddModule<T>(T module) where T : AppModule
    {
        _modules.AddModule(module);
        return this;
    }

    public AppBuilder RemoveModule<T>() where T : AppModule
    {
        _modules.RemoveModule<T>();
        return this;
    }

    public AppBuilder ConfigureModule<T>(Action<T> configure) where T : AppModule
    {
        var module = _modules
            .GetModule<T>()
            .OrElseThrow(() => new RequiredModuleMissingException(typeof(T)));
        configure(module);
        return this;
    }

    public AppDescription Build() => new(_serviceName, _modules, _assemblies);
}

public static class AppBuilderExtensions
{
    public static AppBuilder AddModule<T>(this AppBuilder builder) where T : AppModule, new() =>
        builder.AddModule(new T());
}
