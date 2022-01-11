using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IDataAccessImplementation dataAccessImplementation,
        bool usesPublisher,
        bool usesConsumer)
    {
        dataAccessImplementation.AddUtilityServices(services);
        dataAccessImplementation.AddUnitOfWork(services);
        dataAccessImplementation.AddTransactionManager(services);
        if (usesPublisher)
        {
            dataAccessImplementation.AddOutbox(services);
        }
        if (usesConsumer)
        {
            dataAccessImplementation.AddIdempotenceManager(services);
        }
        return services;
    }
}
