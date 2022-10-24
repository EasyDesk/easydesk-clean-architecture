using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public class DataAccessModule : AppModule
{
    public DataAccessModule(IDataAccessImplementation implementation)
    {
        Implementation = implementation;
    }

    public IDataAccessImplementation Implementation { get; }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        Implementation.AddMainDataAccessServices(services, app);
    }
}

public static class DataAccessModuleExtensions
{
    public static AppBuilder AddDataAccess(this AppBuilder builder, IDataAccessImplementation implementation)
    {
        return builder.AddModule(new DataAccessModule(implementation));
    }
}
