using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus;

public sealed class TestBusEndpointsManager : IAsyncDisposable
{
    private readonly IList<ITestBusEndpoint> _busEndpoints = [];
    private readonly ITestHost _host;
    private readonly TestTenantManager _tenantManager;

    public TestBusEndpointsManager(ITestHost host, TestTenantManager tenantManager)
    {
        _host = host;
        _tenantManager = tenantManager;
    }

    public ITestBusEndpoint NewBusEndpoint(string? inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var busEndpoint = RebusTestBusEndpoint.CreateFromServices(
            context: _host.LifetimeScope,
            testTenantNavigator: _tenantManager,
            inputQueueAddress: inputQueueAddress ?? GenerateNewRandomAddress(),
            defaultTimeout: defaultTimeout);

        _busEndpoints.Add(busEndpoint);

        return busEndpoint;
    }

    private static string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";

    public async ValueTask DisposeAsync()
    {
        foreach (var endpoint in _busEndpoints)
        {
            await endpoint.DisposeAsync();
        }
    }
}
