﻿using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using System.Collections;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public sealed class ModulesCollection : IEnumerable<AppModule>
{
    private readonly Dictionary<Type, AppModule> _modules = [];

    public void AddModule<T>(T module) where T : AppModule =>
        _modules[typeof(T)] = module;

    public void RemoveModule<T>() where T : AppModule =>
        _modules.Remove(typeof(T));

    public Option<T> GetModule<T>() where T : AppModule =>
        _modules.GetOption(typeof(T)).Map(m => (T)m);

    public IEnumerator<AppModule> GetEnumerator() =>
        _modules.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
