using AutoMapper;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Application.Mapping
{
    public class DefaultMappingProfile : Profile
    {
        public DefaultMappingProfile(params Type[] assemblyTypes) : this(assemblyTypes.AsEnumerable())
        {
        }

        public DefaultMappingProfile(IEnumerable<Type> assemblyTypes)
        {
            ReflectionUtils.InstancesOfSubtypesOf<IMapping>(assemblyTypes)
                .ForEach(m => m.ConfigureProfile(this));
        }
    }
}
