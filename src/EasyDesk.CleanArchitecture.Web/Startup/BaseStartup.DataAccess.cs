using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    protected abstract IDataAccessImplementation DataAccessImplementation { get; }

    private void AddDataAccess(IServiceCollection services)
    {
        services.AddDataAccess(DataAccessImplementation, UsesPublisher, UsesConsumer);
    }
}
