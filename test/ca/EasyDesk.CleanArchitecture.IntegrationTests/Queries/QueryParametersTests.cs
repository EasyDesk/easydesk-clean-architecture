using EasyDesk.CleanArchitecture.IntegrationTests.Api;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class QueryParametersTests : SampleIntegrationTest
{
    public QueryParametersTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task OptionInQuery_ShouldBeCorrectlyParsed_WhenEmpty()
    {
        var response = Http
            .GetOptionInQuery(None)
            .Send()
            .AsVerifiable();

        await Verify(response);
    }

    [Fact]
    public async Task OptionInQuery_ShouldBeCorrectlyParsed_WhenSome()
    {
        var response = Http
            .GetOptionInQuery(Some("Hello World!"))
            .Send()
            .AsVerifiable();

        await Verify(response);
    }
}
