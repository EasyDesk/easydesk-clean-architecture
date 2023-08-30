using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Seeding;

internal class AutoScopingDispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AsyncAction<IServiceProvider> _setupScope;

    public AutoScopingDispatcher(IServiceProvider serviceProvider, AsyncAction<IServiceProvider> setupScope)
    {
        _serviceProvider = serviceProvider;
        _setupScope = setupScope;
    }

    public async Task<Result<R>> Dispatch<X, R>(IDispatchable<X> dispatchable, AsyncFunc<X, R> mapper)
    {
        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            await _setupScope(scope.ServiceProvider);
            return await scope.ServiceProvider.GetRequiredService<IDispatcher>().Dispatch(dispatchable, mapper);
        }
    }
}
