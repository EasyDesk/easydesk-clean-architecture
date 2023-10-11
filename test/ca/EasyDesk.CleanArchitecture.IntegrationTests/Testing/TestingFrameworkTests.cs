using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Lifetime;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.Commons.Tasks;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Testing;

internal class IntegrationTestExample : SampleIntegrationTest
{
    private const int Count = 50;
    private const int PageSize = 10;

    public IntegrationTestExample(SampleAppTestsFixture factory) : base(factory)
    {
    }

    protected override async Task OnInitialization()
    {
        TenantNavigator.MoveToTenant(Fixture.TestData.TestTenant);
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();
    }

    public HttpSingleRequestExecutor<PersonDto> CreatePerson(int id) => Http
        .CreatePerson(new($"FirstName_{id}", $"LastName_{id}", new LocalDate(1997, 1, 1).PlusDays(id), AddressDto.Create("a place")));

    public async Task CreateAndCheckPeopleAndPets(bool skipWaits = false)
    {
        for (var i = 0; i < Count; i++)
        {
            await CreatePerson(i)
                .Send()
                .EnsureSuccess();
        }
        if (!skipWaits)
        {
            await Http
                .GetPeople()
                .SetPageSize(PageSize)
                .PollUntil(people => people.Count() == Count, Duration.FromMilliseconds(20), Duration.FromSeconds(15))
                .EnsureSuccess();
            await WebService.WaitConditionUnderTenant<SampleAppContext>(
                Fixture.TestData.TestTenant,
                async context => await context.Pets.CountAsync() == Count);
        }
    }
}

public class TestFixtureLifecycleTests
{
    private async Task UsingFixture(AsyncAction<SampleAppTestsFixture> action) =>
        await AsyncLifetime.UsingAsyncLifetime(() => new SampleAppTestsFixture(), action);

    private async Task RunTest(SampleAppTestsFixture fixture, bool skipWaits = false)
    {
        await AsyncLifetime.UsingAsyncLifetime(() => new IntegrationTestExample(fixture), async integrationTest =>
        {
            await integrationTest.CreateAndCheckPeopleAndPets(skipWaits);
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle()
    {
        await UsingFixture(async fixture =>
        {
            for (var i = 0; i < 5; i++)
            {
                await RunTest(fixture);
            }
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle_SkippingWaits()
    {
        await UsingFixture(async fixture =>
        {
            for (var i = 0; i < 5; i++)
            {
                await RunTest(fixture, skipWaits: true);
            }
        });
    }

    [Fact]
    public async Task TestFixtureLifecycle_WithParallelism()
    {
        async Task Run(Duration delay)
        {
            await Task.Delay(delay.ToTimeSpan());
            await UsingFixture(async fixture =>
            {
                for (var i = 0; i < 5; i++)
                {
                    await RunTest(fixture);
                }
            });
        }
        await Task.WhenAll(
            Run(Duration.FromSeconds(1)),
            Run(Duration.FromSeconds(2)),
            Run(Duration.FromSeconds(5)),
            Run(Duration.FromSeconds(10)));
    }

    [Fact]
    public async Task TestFixtureLifecycle_WithParallelism_SkippingWaits()
    {
        async Task Run()
        {
            await UsingFixture(async fixture =>
            {
                for (var i = 0; i < 5; i++)
                {
                    await RunTest(fixture, skipWaits: true);
                }
            });
        }
        await Task.WhenAll(Run(), Run());
    }
}
