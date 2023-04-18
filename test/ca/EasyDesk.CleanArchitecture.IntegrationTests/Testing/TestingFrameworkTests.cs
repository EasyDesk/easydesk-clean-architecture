using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Application.IncomingCommands;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Testing;

internal class IntegrationTestExample : SampleIntegrationTest
{
    public static readonly TenantId Tenant = TenantId.New("test-tenant-test");
    private static readonly UserId _adminId = UserId.New("test-admin-test");

    public IntegrationTestExample(SampleAppTestsFixture factory) : base(factory)
    {
    }

    public ITestWebService GetService() => WebService;

    public HttpTestHelper GetHttp() => Http;

    public ITestBus GetNewBus(string? inputQueue = null) => NewBus(inputQueue);

    protected override void ConfigureRequests(HttpRequestBuilder req) => req
        .Tenant(Tenant)
        .AuthenticateAs(_adminId);

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(Tenant));
        await WebService.WaitUntilTenantExists(TenantId.New(Tenant));
        await Http.AddAdmin().Send().EnsureSuccess();
    }

    public HttpSingleRequestExecutor<PersonDto> CreatePerson(int id) => Http
        .CreatePerson(new($"FirstName_{id}", $"LastName_{id}", new LocalDate(1997, 1, 1).PlusDays(id), new("a place")));

    public static async Task CreateAndCheckPeopleAndPets(int count, IntegrationTestExample integrationTest, bool skipWaits = false)
    {
        var http = integrationTest.GetHttp();
        var webService = integrationTest.GetService();
        for (var i = 0; i < count; i++)
        {
            await integrationTest.CreatePerson(i)
                .Send()
                .EnsureSuccess();
        }
        if (!skipWaits)
        {
            for (var i = 0; i < 5; i++)
            {
                await http
                    .GetPeople()
                    .PollUntil(people => people.Count() == count, Duration.FromMilliseconds(20), Duration.FromSeconds(15))
                    .EnsureSuccess();
                await webService.WaitConditionUnderTenant<SampleAppContext>(
                    TenantId.New(Tenant),
                    async context => await context.Pets.CountAsync() == count);
            }
        }
    }
}

public class TestFixtureLifecycleTests
{
    [Fact]
    public async Task TestFixtureLifecycle()
    {
        await using var fixture = new SampleAppTestsFixture();
        await fixture.InitializeAsync();
        for (var ii = 0; ii < 10; ii++)
        {
            await using var integrationTest = new IntegrationTestExample(fixture);
            await integrationTest.InitializeAsync();
            await IntegrationTestExample.CreateAndCheckPeopleAndPets(150, integrationTest);
        }
    }
}

public class TestFixtureLifecycleTests_SkippingWaits
{
    [Fact]
    public async Task TestFixtureLifecycle_SkippingWaits()
    {
        await using var fixture = new SampleAppTestsFixture();
        await fixture.InitializeAsync();
        for (var ii = 0; ii < 10; ii++)
        {
            await using var integrationTest = new IntegrationTestExample(fixture);
            await integrationTest.InitializeAsync();
            await IntegrationTestExample.CreateAndCheckPeopleAndPets(150, integrationTest, skipWaits: true);
        }
    }
}

public class TestFixtureLifecycleWithParallelismTests
{
    [Fact]
    public async Task TestFixtureLifecycle_WithParallelism()
    {
        var action = async (Duration delay) =>
        {
            await Task.Delay(delay.ToTimeSpan());
            await using var fixture = new SampleAppTestsFixture();
            await fixture.InitializeAsync();
            for (var ii = 0; ii < 10; ii++)
            {
                await using var integrationTest = new IntegrationTestExample(fixture);
                await integrationTest.InitializeAsync();
                await IntegrationTestExample.CreateAndCheckPeopleAndPets(150, integrationTest);
            }
        };
        await Task.WhenAll(
            action(Duration.FromSeconds(1)),
            action(Duration.FromSeconds(2)),
            action(Duration.FromSeconds(5)),
            action(Duration.FromSeconds(10)));
    }
}

public class TestFixtureLifecycleWithParallelismTests_SkippingWaits
{
    [Fact]
    public async Task TestFixtureLifecycle_WithParallelism_SkippingWaits()
    {
        var action = async () =>
        {
            await using var fixture = new SampleAppTestsFixture();
            await fixture.InitializeAsync();
            for (var ii = 0; ii < 10; ii++)
            {
                await using var integrationTest = new IntegrationTestExample(fixture);
                await integrationTest.InitializeAsync();
                await IntegrationTestExample.CreateAndCheckPeopleAndPets(150, integrationTest, skipWaits: true);
            }
        };
        await Task.WhenAll(action(), action());
    }
}
