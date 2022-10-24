using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.DependencyInjection.Modules;

public abstract class AppModule
{
    public virtual void BeforeServiceConfiguration(AppDescription app)
    {
    }

    public virtual void AfterServiceConfiguration(AppDescription app)
    {
    }

    public abstract void ConfigureServices(IServiceCollection services, AppDescription app);
}
