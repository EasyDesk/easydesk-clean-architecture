using EasyDesk.SampleApp.Application.Commands;
using EasyDesk.SampleApp.Domain.Aggregates.PersonAggregate;
using NodaTime;
using NSubstitute;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

[UsesVerify]
public class SampleAppUnitTestsTest
{
    private const string TestSurname = "TestSurname";
    private const string TestName = "TestName";
    private static readonly LocalDate _testDate = new(1970, 1, 1);

    [Fact]
    public async Task CreatePersonCommandTest()
    {
        var personRepository = Substitute.For<IPersonRepository>();
        var handler = new CreatePerson.Handler(personRepository);
        var result = await handler.Handle(new CreatePerson.Command(TestName, TestSurname, _testDate));
        await Verify(result);
    }
}
