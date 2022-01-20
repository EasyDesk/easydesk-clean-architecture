using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Features;

public class AppDescription
{
    private readonly IImmutableDictionary<Type, IAppFeature> _appFeatures;

    public AppDescription(IEnumerable<IAppFeature> appFeatures)
    {
        _appFeatures = appFeatures.ToImmutableDictionary(f => f.GetType());
    }

    public Type WebAssemblyMarker { get; init; }

    public Type ApplicationAssemblyMarker { get; init; }

    public Type InfrastructureAssemblyMarker { get; init; }

    public string Name { get; init; }

    public Option<T> GetFeature<T>() where T : IAppFeature => _appFeatures.GetOption(typeof(T)).Map(f => (T)f);

    public T RequireFeature<T>() where T : IAppFeature => GetFeature<T>()
        .OrElseThrow(() => new InvalidOperationException($"Missing required feature of type {typeof(T).Name}"));

    public bool HasFeature<T>() where T : IAppFeature => _appFeatures.ContainsKey(typeof(T));

    public void ConfigureServices(IServiceCollection services)
    {
        _appFeatures.Values.ForEach(f => f.ConfigureServices(services, this));
    }
}
