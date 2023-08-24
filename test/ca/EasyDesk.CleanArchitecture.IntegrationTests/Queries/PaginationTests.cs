using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Services;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class PaginationTests : SampleIntegrationTest
{
    private const int InitialPopulationSize = 300;
    private static readonly TenantId _tenant = TenantId.New("test-tenant-a");

    public PaginationTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override async Task OnInitialization()
    {
        var bus = NewBus();
        await bus.Send(new CreateTenant(_tenant));
        await WebService.WaitUntilTenantExists(_tenant);

        TenantNavigator.MoveToTenant(_tenant);
        AuthenticateAs(TestAgents.Admin);

        await Http.AddAdmin().Send().EnsureSuccess();
        foreach (var i in Enumerable.Range(0, InitialPopulationSize))
        {
            await CreatePerson(i).Send().EnsureSuccess();
        }
    }

    private HttpSingleRequestExecutor<PersonDto> CreatePerson(int index) => Http
        .CreatePerson(
            new(
                FirstName: $"Foo{index:0000}",
                LastName: $"Bar{index:0000}",
                DateOfBirth: new LocalDate(2000, 1, 1).PlusDays(index),
                Residence: AddressDto.Create("some name idk", "street", index.ToString())));

    private HttpSingleRequestExecutor<IEnumerable<PersonDto>> GetPeople(int pageSize) => Http
        .Get<IEnumerable<PersonDto>>(PersonRoutes.GetPeople)
        .SetPageSize(pageSize);

    [Fact]
    public async Task DefaultPageSize_ShouldBeConfigurable()
    {
        var response = await Http
            .GetPeople()
            .SinglePage()
            .AsVerifiable();
        await Verify(response);
    }

    [Fact]
    public async Task ShouldGetAnyPage()
    {
        var response = await Http
            .GetPeople()
            .SinglePage(1)
            .AsVerifiable();
        await Verify(response);
    }

    [Fact]
    public async Task MaxPageSize_ShouldBeConfigurable()
    {
        var response = await GetPeople(int.MaxValue)
            .Send()
            .AsVerifiable();
        await Verify(response);
    }

    [Fact]
    public async Task MinPageSize_ShouldBeOne()
    {
        var response = await GetPeople(int.MinValue)
            .Send()
            .AsVerifiable();
        await Verify(response);
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldWorkWithAnyPageSize()
    {
        var response = await Http.GetPeople().SetPageSize(1).Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSize()
    {
        var response = await Http.GetPeople().SetPageIndex(3).SetPageSize(5).Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSizeEvenIfOutOfBounds()
    {
        var response = await Http.GetPeople().SetPageIndex(int.MaxValue).SetPageSize(int.MaxValue).Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSizeEvenIfOutOfBoundsInNegative()
    {
        var response = await Http.GetPeople().SetPageIndex(int.MinValue).SetPageSize(int.MinValue).Send().AsVerifiableEnumerable();

        await Verify(response);
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldNotWorkWithInvalidInput()
    {
        var response = await Http
        .GetPeople()
        .SetPageSize("foo")
        .SetPageIndex("bar")
        .SinglePage()
        .AsVerifiable();

        await Verify(response);
    }
}
