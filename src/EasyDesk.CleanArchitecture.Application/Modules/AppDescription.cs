using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Modules;

public class AppDescription
{
    private readonly IImmutableDictionary<Type, IAppModule> _modules;

    public AppDescription(IEnumerable<IAppModule> modules)
    {
        _modules = modules.ToImmutableDictionary(f => f.GetType());
    }

    public Type WebAssemblyMarker { get; init; }

    public Type ApplicationAssemblyMarker { get; init; }

    public Type InfrastructureAssemblyMarker { get; init; }

    public string Name { get; init; }

    public Option<T> GetModule<T>() where T : IAppModule => _modules.GetOption(typeof(T)).Map(m => (T)m);

    public T RequireModule<T>() where T : IAppModule => GetModule<T>()
        .OrElseThrow(() => new InvalidOperationException($"Missing required module of type {typeof(T).Name}"));

    public bool HasModule<T>() where T : IAppModule => _modules.ContainsKey(typeof(T));

    public void ConfigureServices(IServiceCollection services)
    {
        _modules.Values.ForEach(m => m.ConfigureServices(services, this));
    }
}
