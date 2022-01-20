using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Features;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data;

public class DataAccessFeature : IAppFeature
{
    public DataAccessFeature(IDataAccessImplementation implementation)
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

public static class DataAccessFeatureExtensions
{
    public static AppBuilder AddDataAccess(this AppBuilder builder, IDataAccessImplementation implementation)
    {
        return builder.AddFeature(new DataAccessFeature(implementation));
    }
}
