using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public class SampleTestData : WebServiceSeeder<SampleAppTestsFixture>
{
    public TenantId TestTenant { get; } = new("test-tenant");

    public int OperationsRun { get; private set; }

    public SampleTestData(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public override async Task Seed()
    {
        await RunOperation(() => DefaultBusEndpoint.Send(new CreateTenant(TestTenant)));
        await WebService.WaitUntilTenantExists(TestTenant);
    }

    private async Task RunOperation(AsyncAction operation)
    {
        await operation();
        OperationsRun++;
    }
}
