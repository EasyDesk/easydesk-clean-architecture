using EasyDesk.Commons.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public class AppDescription
{
    private readonly string _assemblyPrefix;
    private readonly ModulesCollection _modules;
    private readonly IImmutableDictionary<CleanArchitectureLayer, Assembly> _layers;

    public AppDescription(
        string name,
        string assemblyPrefix,
        ModulesCollection modules,
        IImmutableDictionary<CleanArchitectureLayer, Assembly> layers)
    {
        Name = name;
        _assemblyPrefix = assemblyPrefix;
        _modules = modules;
        _layers = layers;
    }

    public string Name { get; }

    public Assembly GetLayerAssembly(CleanArchitectureLayer layer) =>
        _layers.Update(layer, It, () => Assembly.Load($"{_assemblyPrefix}.{layer}"))[layer];

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
