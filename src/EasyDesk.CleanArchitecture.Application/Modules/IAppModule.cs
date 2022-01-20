using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Modules;

public interface IAppModule
{
    void ConfigureServices(IServiceCollection services, AppDescription app);
}
