using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Application.Events;
using EasyDesk.SampleApp.Application.Queries;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Commands;

[UsesVerify]
public class CreatePersonTests : SampleIntegrationTest
{
    private const string Uri = "people";

    private readonly CreatePersonBodyDto _body = new(
        FirstName: "Foo",
        LastName: "Bar",
        DateOfBirth: new LocalDate(1996, 2, 2));

    public CreatePersonTests(SampleApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePersonShouldSucceedAsRestCall()
    {
        var response = await Http
            .Post(Uri, _body)
            .AsVerifiableResponse<PersonDto>();

        await Verify(response);
    }

    [Fact]
    public async Task CreatePersonShouldSucceedAsAsyncMessage()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        await bus.Send<CreatePerson, PersonSnapshot>(new CreatePerson("Foo", "Bar", new LocalDate(1996, 2, 2)));

        await bus.WaitForMessageOrFail<PersonCreated>();
    }

    [Fact]
    public async Task CreatePersonShouldEmitAnEvent()
    {
        await using var bus = NewBus();
        await bus.Subscribe<PersonCreated>();

        var response = await Http.Post(Uri, _body).AsContentOnly<PersonDto>();

        await bus.WaitForMessageOrFail(new PersonCreated(response.Data.Id));
    }
}
