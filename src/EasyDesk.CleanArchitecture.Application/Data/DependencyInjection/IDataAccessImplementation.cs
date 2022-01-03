using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public interface IDataAccessImplementation
{
    void AddUtilityServices(IServiceCollection services);

    void AddUnitOfWork(IServiceCollection services);

    void AddTransactionManager(IServiceCollection services);

    void AddOutbox(IServiceCollection services);

    void AddIdempotenceManager(IServiceCollection services);

    public class Default : IDataAccessImplementation
    {
        public void AddUtilityServices(IServiceCollection services)
        {
        }

        public void AddUnitOfWork(IServiceCollection services)
        {
        }

        public void AddTransactionManager(IServiceCollection services)
        {
        }

        public void AddOutbox(IServiceCollection services)
        {
        }

        public void AddIdempotenceManager(IServiceCollection services)
        {
        }
    }
}
