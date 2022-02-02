using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public interface IDataAccessImplementation
{
    void AddMainDataAccessServices(IServiceCollection services, AppDescription app);

    void AddOutbox(IServiceCollection services, AppDescription app);

    void AddIdempotenceManager(IServiceCollection services, AppDescription app);

    void AddRoleBasedPermissionsProvider(IServiceCollection services, AppDescription app);

    void AddRoleManager(IServiceCollection services, AppDescription app);
}
