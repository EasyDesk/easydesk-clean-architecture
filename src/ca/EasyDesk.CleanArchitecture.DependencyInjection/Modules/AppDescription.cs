using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class AppDescription
{
    private readonly string _assemblyPrefix;
    private readonly ModulesCollection _modules;
    private IImmutableDictionary<CleanArchitectureLayer, IImmutableSet<Assembly>> _layers;

    public AppDescription(
        string name,
        string assemblyPrefix,
        ModulesCollection modules,
        IImmutableDictionary<CleanArchitectureLayer, IImmutableSet<Assembly>> layers)
    {
        Name = name;
        _assemblyPrefix = assemblyPrefix;
        _modules = modules;
        _layers = layers;
    }

    public string Name { get; }

    public IEnumerable<Assembly> GetLayerAssemblies(CleanArchitectureLayer layer)
    {
        _layers = _layers.Update(layer, It, () => Set(Assembly.Load($"{_assemblyPrefix}.{layer}")));
        return _layers[layer];
    }

    public Option<T> GetModule<T>() where T : AppModule =>
        _modules.GetModule<T>();

    public T RequireModule<T>() where T : AppModule =>
        GetModule<T>().OrElseThrow(() => new RequiredModuleMissingException(typeof(T)));

    public bool HasModule<T>() where T : AppModule =>
        GetModule<T>().IsPresent;

    public void ConfigureServices(IServiceCollection services)
    {
        _modules.ForEach(m => m.BeforeServiceConfiguration(this));
        _modules.ForEach(m => m.ConfigureServices(services, this));
        _modules.ForEach(m => m.AfterServiceConfiguration(this));
    }
}
