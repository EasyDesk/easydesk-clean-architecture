using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class PaginationTests : SampleIntegrationTest
{
    private const string TenantId = "test-tenant-a";
    private const string AdminId = "test-admin-a";
    private const int InitialPopulationSize = 300;

    public PaginationTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req) =>
        req.Tenant(TenantId).AuthenticateAs(AdminId);

    private HttpSingleRequestExecutor<PersonDto> CreatePerson(int index) => Http
        .CreatePerson(
            new(
                FirstName: $"Foo{index:0000}",
                LastName: $"Bar{index:0000}",
                DateOfBirth: new LocalDate(2000, 1, 1).PlusDays(index)));

    private HttpSingleRequestExecutor<IEnumerable<PersonDto>> GetPeople(int pageSize) => Http
        .Get<IEnumerable<PersonDto>>(PersonRoutes.GetPeople)
        .SetPageSize(pageSize);

    protected override async Task OnInitialization()
    {
        foreach (var i in Enumerable.Range(0, InitialPopulationSize))
        {
            await CreatePerson(i).Send().EnsureSuccess();
        }
    }

    [Fact]
    public async Task DefaultPageSize_ShouldBeConfigurable()
    {
        var response = await Http
            .Get<IEnumerable<PersonDto>>(PersonRoutes.GetPeople)
            .Send()
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
}
