using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public class DataAccessModule : IAppModule
{
    public DataAccessModule(IDataAccessImplementation implementation)
    {
        Implementation = implementation;
    }

    public IDataAccessImplementation Implementation { get; }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        Implementation.AddUtilityServices(services, app);
        Implementation.AddUnitOfWork(services, app);
        Implementation.AddTransactionManager(services, app);
    }
}

public static class DataAccessModuleExtensions
{
    public static AppBuilder AddDataAccess(this AppBuilder builder, IDataAccessImplementation implementation)
    {
        return builder.AddModule(new DataAccessModule(implementation));
    }
}
