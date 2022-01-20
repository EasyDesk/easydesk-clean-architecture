using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Modules;

public class AppBuilder
{
    private readonly Dictionary<Type, IAppModule> _modules = new();

    public AppBuilder AddModule<T>(T module) where T : IAppModule
    {
        _modules.Merge(typeof(T), module, (_, f) => f);
        return this;
    }

    public AppBuilder RemoveModule<T>() where T : IAppModule
    {
        if (_modules.ContainsKey(typeof(T)))
        {
            _modules.Remove(typeof(T));
        }
        return this;
    }

    public AppDescription Build(string name, Type web, Type application, Type infrastructure)
    {
        return new(_modules.Values)
        {
            Name = name,
            WebAssemblyMarker = web,
            ApplicationAssemblyMarker = application,
            InfrastructureAssemblyMarker = infrastructure
        };
    }
}
