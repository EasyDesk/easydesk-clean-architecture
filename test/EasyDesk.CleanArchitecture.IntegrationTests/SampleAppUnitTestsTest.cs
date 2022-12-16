using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;
using NSubstitute;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
public class SampleAppUnitTestsTest
{
    private const string TestCreatedBy = "test-admin-id";
    private const string TestSurname = "TestSurname";
    private const string TestName = "TestName";
    private static readonly LocalDate _testDate = new(1970, 1, 1);

    [Fact]
    public async Task CreatePersonCommandTest()
    {
        var personRepository = Substitute.For<IPersonRepository>();
        var contextProvider = Substitute.For<IContextProvider>();
        contextProvider.Context.ReturnsForAnyArgs(new AuthenticatedRequestContext(new UserInfo(TestCreatedBy)));
        var handler = new CreatePersonHandler(personRepository, contextProvider);
        var result = await handler.Handle(new CreatePerson(TestName, TestSurname, _testDate));
        await Verify(result);
    }
}
