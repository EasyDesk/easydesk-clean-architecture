using EasyDesk.CleanArchitecture.Application.Dispatching;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Seeding;

internal class AutoScopingDispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Action<IServiceProvider> _setupScope;

    public AutoScopingDispatcher(IServiceProvider serviceProvider, Action<IServiceProvider> setupScope)
    {
        _serviceProvider = serviceProvider;
        _setupScope = setupScope;
    }

    public async Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper)
    {
        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            _setupScope(scope.ServiceProvider);
            return await scope.ServiceProvider.GetRequiredService<IDispatcher>().Dispatch(dispatchable, mapper);
        }
    }
}
