using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.IntegrationTests.Seeders;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Paginated;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Single;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class PaginationTests : SampleAppIntegrationTest
{
    private const int InitialPopulationSize = 50;

    public PaginationTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    protected override void ConfigureSession(SessionConfigurer configurer)
    {
        configurer.SetDefaultAgent(TestAgents.Admin);
        configurer.SetDefaultTenant(SampleSeeder.Data.TestTenant);
    }

    protected override async Task OnInitialization()
    {
        await Session.Http.AddAdmin().Send().EnsureSuccess();
        foreach (var i in Enumerable.Range(0, InitialPopulationSize))
        {
            await CreatePerson(i).Send().EnsureSuccess();
        }
    }

    private HttpSingleRequestExecutor<PersonDto> CreatePerson(int index) => Session.Http
        .CreatePerson(new()
        {
            FirstName = $"Foo{index:0000}",
            LastName = $"Bar{index:0000}",
            DateOfBirth = new LocalDate(2000, 1, 1).PlusDays(index),
            Residence = AddressDto.Create("some name idk", "street", index.ToString()),
        });

    private HttpSingleRequestExecutor<IEnumerable<PersonDto>> GetPeople(int pageSize) => Session.Http
        .Get<IEnumerable<PersonDto>>(PersonRoutes.GetPeople)
        .With(x => x.PageSize(pageSize));

    [Fact]
    public async Task DefaultPageSize_ShouldBeConfigurable()
    {
        await Session.Http
            .GetPeople()
            .SinglePage()
            .Verify();
    }

    [Fact]
    public async Task ShouldGetAnyPage()
    {
        await Session.Http
            .GetPeople()
            .SinglePage(1)
            .Verify();
    }

    [Fact]
    public async Task MaxPageSize_ShouldBeConfigurable()
    {
        await GetPeople(int.MaxValue)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task MinPageSize_ShouldBeOne()
    {
        await GetPeople(int.MinValue)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldWorkWithAnyPageSize()
    {
        await Session.Http
            .GetPeople()
            .With(x => x.PageSize(1))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSize()
    {
        await Session.Http
            .GetPeople()
            .With(x => x
                .PageIndex(3)
                .PageSize(5))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSizeEvenIfOutOfBounds()
    {
        await Session.Http
            .GetPeople()
            .With(x => x
                .PageIndex(int.MaxValue)
                .PageSize(int.MaxValue))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldStartWithGivenPageIndexAndSizeEvenIfOutOfBoundsInNegative()
    {
        await Session.Http
            .GetPeople()
            .With(x => x
                .PageIndex(int.MinValue)
                .PageSize(int.MinValue))
            .Send()
            .Verify();
    }

    [Fact]
    public async Task HttpRequestBuilder_ShouldNotWorkWithInvalidInput()
    {
        await Session.Http
            .GetPeople()
            .With(x => x
                .PageSize("foo")
                .PageIndex("bar"))
            .SinglePage()
            .Verify();
    }
}
