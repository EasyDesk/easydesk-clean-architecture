using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;

public interface IDataAccessImplementation
{
    void ConfigurePipeline(PipelineBuilder pipeline);

    void AddMainDataAccessServices(IServiceCollection services, AppDescription app);

    void AddMessagingUtilities(IServiceCollection services, AppDescription app);

    void AddRolesManagement(IServiceCollection services, AppDescription app);

    void AddMultitenancy(IServiceCollection services, AppDescription app);

    void AddSagas(IServiceCollection services, AppDescription app);

    void AddAuditing(IServiceCollection services, AppDescription app);

    void AddApiKeysManagement(IServiceCollection services, AppDescription app);
}
