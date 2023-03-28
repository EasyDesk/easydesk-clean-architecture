using EasyDesk.Commons.Collections;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public class AppBuilder
{
    private readonly ModulesCollection _modules = new();
    private readonly Dictionary<CleanArchitectureLayer, Assembly> _layers = new();
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
        _layers[layer] = layerAssembly;
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

    public AppDescription Build() => new(_serviceName, _assemblyPrefix, _modules, _layers.ToEquatableMap());
}

public static class AppBuilderExtensions
{
    public static AppBuilder AddModule<T>(this AppBuilder builder) where T : AppModule, new() =>
        builder.AddModule(new T());
}
