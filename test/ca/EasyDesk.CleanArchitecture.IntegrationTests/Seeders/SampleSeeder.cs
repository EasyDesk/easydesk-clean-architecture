using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Seeding;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Seeders;

public record SampleTestData(
    int OperationsRun,
    TenantId TestTenant);

public class SampleSeeder : WebServiceSeeder<SampleAppTestsFixture, SampleTestData>
{
    private static readonly TenantId _testTenant = new("test-tenant");

    public SampleSeeder(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    public override async Task<SampleTestData> Seed()
    {
        var operationsRun = 0;

        async Task RunOperation(AsyncAction operation)
        {
            await operation();
            operationsRun++;
        }

        await RunOperation(() => DefaultBusEndpoint.Send(new CreateTenant(_testTenant)));
        await WebService.WaitUntilTenantExists(_testTenant);

        return new(operationsRun, _testTenant);
    }
}
