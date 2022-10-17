using EasyDesk.CleanArchitecture.Application.Modules;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Configuration;

namespace EasyDesk.CleanArchitecture.Web.Startup.Modules;
public class AsyncApiModule : AppModule
{
    private readonly Action<IAsyncApiGenerationOptionsBuilder> _configure;

    public AsyncApiModule(Action<IAsyncApiGenerationOptionsBuilder> configure = null)
    {
        _configure = configure;
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddAsyncApiGeneration(options =>
        {
            SetupAsyncApiDocs(app, options);
            _configure?.Invoke(options);
        });
        services.AddAsyncApiUI("neuroglia");
    }

    private void SetupAsyncApiDocs(AppDescription app, IAsyncApiGenerationOptionsBuilder options)
    {
    }
}
