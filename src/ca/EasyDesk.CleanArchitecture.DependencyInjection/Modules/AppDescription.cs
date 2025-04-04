﻿using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class AppDescription
{
    private readonly ModulesCollection _modules;

    public AppDescription(
        string name,
        ModulesCollection modules,
        IFixedSet<Assembly> assemblies)
    {
        Name = name;
        _modules = modules;
        Assemblies = assemblies;
    }

    public string Name { get; }

    public IFixedSet<Assembly> Assemblies { get; }

    public Option<T> GetModule<T>() where T : AppModule =>
        _modules.GetModule<T>();

    public T RequireModule<T>() where T : AppModule =>
        GetModule<T>().OrElseThrow(() => new RequiredModuleMissingException(typeof(T)));

    public bool HasModule<T>() where T : AppModule =>
        GetModule<T>().IsPresent;

    public void BeforeServiceConfiguration()
    {
        _modules.ForEach(m => m.BeforeServiceConfiguration(this));
    }

    public void ConfigureRegistry(ServiceRegistry registry)
    {
        _modules.ForEach(m => m.Configure(this, registry));
    }
}
