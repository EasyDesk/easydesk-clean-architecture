using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Features;

public class AppBuilder
{
    private readonly Dictionary<Type, IAppFeature> _features = new();

    public AppBuilder AddFeature<T>(T feature) where T : IAppFeature
    {
        _features.Merge(typeof(T), feature, (_, f) => f);
        return this;
    }

    public AppBuilder RemoveFeature<T>() where T : IAppFeature
    {
        if (_features.ContainsKey(typeof(T)))
        {
            _features.Remove(typeof(T));
        }
        return this;
    }

    public AppDescription Build(string name, Type web, Type application, Type infrastructure)
    {
        return new(_features.Values)
        {
            Name = name,
            WebAssemblyMarker = web,
            ApplicationAssemblyMarker = application,
            InfrastructureAssemblyMarker = infrastructure
        };
    }
}
