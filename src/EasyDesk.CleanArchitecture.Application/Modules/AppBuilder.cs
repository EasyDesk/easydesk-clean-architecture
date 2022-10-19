using System.Collections.Immutable;
using System.Reflection;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Modules;

public class AppBuilder
{
    private IImmutableDictionary<Type, AppModule> _modules = Map<Type, AppModule>();
    private IImmutableDictionary<CleanArchitectureLayer, Assembly> _layers = Map<CleanArchitectureLayer, Assembly>();
    private readonly string _assemblyPrefix;
    private string _serviceName;

    public AppBuilder(string assemblyPrefix)
    {
        _assemblyPrefix = assemblyPrefix;
        _serviceName = assemblyPrefix;
    }

    public AppBuilder WithServiceName(string name)
    {
        _serviceName = name;
        return this;
    }

    public AppBuilder WithDomainLayer(Assembly domainLayer) =>
        WithLayer(CleanArchitectureLayer.Domain, domainLayer);

    public AppBuilder WithApplicationLayer(Assembly applicationLayer) =>
        WithLayer(CleanArchitectureLayer.Application, applicationLayer);

    public AppBuilder WithInfrastructureLayer(Assembly infrastructureLayer) =>
        WithLayer(CleanArchitectureLayer.Infrastructure, infrastructureLayer);

    public AppBuilder WithWebLayer(Assembly webLayer) =>
        WithLayer(CleanArchitectureLayer.Web, webLayer);

    private AppBuilder WithLayer(CleanArchitectureLayer layer, Assembly layerAssembly)
    {
        _layers = _layers.SetItem(layer, layerAssembly);
        return this;
    }

    public AppBuilder AddModule<T>(T module) where T : AppModule
    {
        _modules = _modules.SetItem(typeof(T), module);
        return this;
    }

    public AppBuilder RemoveModule<T>() where T : AppModule
    {
        _modules = _modules.Remove(typeof(T));
        return this;
    }

    public AppDescription Build() => new(_serviceName, _assemblyPrefix, _modules, _layers);
}

public static class AppBuilderExtensions
{
    public static AppBuilder AddModule<T>(this AppBuilder builder) where T : AppModule, new() =>
        builder.AddModule(new T());
}
