using EasyDesk.CleanArchitecture.Testing.Integration.Commons.Polling;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Services;

public class InjectedServiceCheckBuilder<TService>
{
    private static readonly Duration _defaultPollTimeout = Duration.FromSeconds(10);
    private static readonly Duration _defaultQueryInterval = Duration.FromMilliseconds(200);
    private readonly IServiceProvider _serviceProvider;

    public InjectedServiceCheckBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task WaitUntil(
        AsyncFunc<TService, bool> predicate,
        Duration? timeout = null,
        Duration? interval = null)
    {
        var polling = new Polling<bool>(
            async token =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<TService>();
                    return await predicate(service);
                }
            },
            timeout ?? _defaultPollTimeout,
            interval ?? _defaultQueryInterval);
        await polling.PollUntil(It);
    }
}
