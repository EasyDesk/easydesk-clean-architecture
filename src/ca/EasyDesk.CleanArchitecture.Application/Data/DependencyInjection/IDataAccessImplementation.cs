using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public interface IDataAccessImplementation
{
    void ConfigurePipeline(PipelineBuilder pipeline);

    void AddMainDataAccessServices(ServiceRegistry registry, AppDescription app);

    void AddMessagingUtilities(ServiceRegistry registry, AppDescription app);

    void AddRolesManagement(ServiceRegistry registry, AppDescription app);

    void AddMultitenancy(ServiceRegistry registry, AppDescription app);

    void AddSagas(ServiceRegistry registry, AppDescription app);

    void AddAuditing(ServiceRegistry registry, AppDescription app);

    void AddApiKeysManagement(ServiceRegistry registry, AppDescription app);
}
