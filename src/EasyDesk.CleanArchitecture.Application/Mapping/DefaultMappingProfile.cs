using AutoMapper;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Application.Mapping
{
    public class DefaultMappingProfile : Profile
    {
        public DefaultMappingProfile(IEnumerable<Type> assemblyTypes)
        {
            ReflectionUtils.InstancesOfSubtypesOf<IMapping>(assemblyTypes)
                .ForEach(m => m.ConfigureProfile(this));
        }
    }
}
