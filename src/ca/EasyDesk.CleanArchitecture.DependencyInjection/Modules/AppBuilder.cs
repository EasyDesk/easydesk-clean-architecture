using EasyDesk.Commons.Collections.Immutable;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public abstract class AppBuilder : IAppBuilder
{
    private readonly ModulesCollection _modules = [];
    private IFixedSet<Assembly> _assemblies = [];
    private string _name;

    protected AppBuilder(string name)
    {
        _name = name;
    }

    public IAppBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public IAppBuilder WithAssemblies(params Assembly[] assemblies) =>
        WithAssemblies(assemblies.AsEnumerable());

    public IAppBuilder WithAssemblies(IEnumerable<Assembly> assemblies)
    {
        _assemblies = _assemblies.Union(assemblies);
        return this;
    }

    public IAppBuilder AddModule<T>(T module) where T : AppModule
    {
        _modules.AddModule(module);
        return this;
    }

    public IAppBuilder RemoveModule<T>() where T : AppModule
    {
        _modules.RemoveModule<T>();
        return this;
    }

    public IAppBuilder ConfigureModule<T>(Action<T> configure) where T : AppModule
    {
        var module = _modules
            .GetModule<T>()
            .OrElseThrow(() => new RequiredModuleMissingException(typeof(T)));
        configure(module);
        return this;
    }

    protected AppDescription BuildAppDescription() => new(_name, _modules, _assemblies);

    public abstract Task<int> Run();
}
