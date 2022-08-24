using AutoMapper;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Mapping;

public class DefaultMappingProfile : Profile
{
    public DefaultMappingProfile(params Assembly[] assemblies) : this(assemblies.AsEnumerable())
    {
    }

    public DefaultMappingProfile(IEnumerable<Assembly> assemblies)
    {
        new AssemblyScanner()
            .FromAssemblies(assemblies)
            .NonAbstract()
            .SubtypesOrImplementationsOf<IMapping>()
            .FindTypes()
            .Select(Activator.CreateInstance)
            .Cast<IMapping>()
            .ForEach(m => m.ConfigureProfile(this));
    }
}
