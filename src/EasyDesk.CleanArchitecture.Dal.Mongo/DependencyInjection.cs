using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasyDesk.CleanArchitecture.Dal.Mongo;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoDataAccess<TService, TImplementation>(this IServiceCollection services, string connectionString, string databaseName, params Type[] assemblyTypes)
        where TService : class
        where TImplementation : class, TService
    {
        MongoUtils.ApplyConfigurationFromAssemblies(assemblyTypes);
        return services
            .AddScoped(_ => new MongoContext(connectionString, databaseName))
            .AddScoped<TService, TImplementation>();
    }
}
