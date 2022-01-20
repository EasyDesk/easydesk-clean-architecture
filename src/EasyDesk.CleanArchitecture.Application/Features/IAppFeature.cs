using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Application.Features;

public interface IAppFeature
{
    void ConfigureServices(IServiceCollection services, AppDescription app);
}
