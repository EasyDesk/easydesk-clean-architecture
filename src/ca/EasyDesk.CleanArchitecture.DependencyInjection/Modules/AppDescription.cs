using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class AppDescription
{
    private readonly ModulesCollection _modules;

    public AppDescription(
        string name,
        ModulesCollection modules,
        IImmutableSet<Assembly> assemblies)
    {
        Name = name;
        _modules = modules;
        Assemblies = assemblies;
    }

    public string Name { get; }

    public IImmutableSet<Assembly> Assemblies { get; }

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
