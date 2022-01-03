using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using System;

namespace EasyDesk.CleanArchitecture.Dal.Mongo;

public static class MongoUtils
{
    public static void ApplyConfigurationFromAssemblies(params Type[] types)
    {
        ReflectionUtils.InstancesOfSubtypesOf<IMongoConfiguration>(types)
            .ForEach(config => config.Apply());
    }
}
