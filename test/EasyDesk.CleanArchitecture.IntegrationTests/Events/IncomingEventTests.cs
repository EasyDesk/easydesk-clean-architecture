using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.SampleApp.Application.IncomingEvents;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.People;
using EasyDesk.SampleApp.Web.Controllers.V_1_0.Pets;
using EasyDesk.Tools.Collections;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Events;

public class IncomingEventTests : SampleIntegrationTest
{
    private const string TenantId = "a-tenant";
    private PersonDto _donator;
    private PetDto _pet;
    private PersonDto _recipient;

    public IncomingEventTests(SampleAppTestsFixture factory) : base(factory)
    {
    }

    protected override void ConfigureRequests(HttpRequestBuilder req)
    {
        req.Tenant(TenantId).AuthenticateAs("an-admin");
    }

    protected override async Task OnInitialization()
    {
        _donator = await Http.CreatePerson(new(
            FirstName: "Foo",
            LastName: "Bar",
            DateOfBirth: new LocalDate(1995, 10, 12)))
        .Send().AsData();

        _pet = await Http
            .CreatePet(_donator.Id, new("Snoopy"))
            .Send()
            .AsData();

        _recipient = await Http.CreatePerson(new(
            FirstName: "FooFrient",
            LastName: "BarFrient",
            DateOfBirth: new LocalDate(1995, 10, 13)))
        .Send().AsData();

        await Http.GetOwnedPets(_recipient.Id).PollUntil(pets => pets.Any()).EnsureSuccess();
    }

    [Fact]
    public async Task PetFreedomDayIncomingEvent_ShouldSucceed()
    {

        var bus = NewBus("pet-freedom-service");
        await bus.Publish(new PetFreedomDayEvent(TenantId));

        await Http.GetOwnedPets(_recipient.Id).PollUntil(pets => pets.IsEmpty()).EnsureSuccess();
    }
}
