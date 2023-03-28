using System.Reflection;

namespace EasyDesk.Commons.Reflection;

public class AssemblyScanner
{
    private readonly HashSet<Assembly> _sourceAssemblies = new();
    private readonly List<Func<Type, bool>> _filters = new();

    public AssemblyScanner FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        _sourceAssemblies.UnionWith(assemblies);
        return this;
    }

    public AssemblyScanner FromAssemblies(params Assembly[] assemblies) =>
        FromAssemblies(assemblies.AsEnumerable());

    public AssemblyScanner FromAssembliesContaining(IEnumerable<Type> markers) =>
        FromAssemblies(markers.Select(t => t.Assembly));

    public AssemblyScanner FromAssembliesContaining(params Type[] markers) =>
        FromAssembliesContaining(markers.AsEnumerable());

    public AssemblyScanner NonAbstract() =>
        Where(t => !(t.IsAbstract || t.IsInterface));

    public AssemblyScanner SubtypesOrImplementationsOf<T>() =>
        SubtypesOrImplementationsOf(typeof(T));

    public AssemblyScanner SubtypesOrImplementationsOf(Type type) =>
        Where(t => t.IsSubtypeOrImplementationOf(type));

    public IEnumerable<Type> FindTypes()
    {
        var allTypes = _sourceAssemblies.SelectMany(a => a.GetTypes());
        return _filters.Aggregate(allTypes, (types, filter) => types.Where(filter));
    }

    public AssemblyScanner Where(Func<Type, bool> filter)
    {
        _filters.Add(filter);
        return this;
    }
}
