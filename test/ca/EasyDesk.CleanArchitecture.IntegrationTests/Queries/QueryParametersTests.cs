using EasyDesk.CleanArchitecture.IntegrationTests.Api;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Extensions;

namespace EasyDesk.CleanArchitecture.IntegrationTests.Queries;

public class QueryParametersTests : SampleIntegrationTest
{
    public QueryParametersTests(SampleAppTestsFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task OptionInQuery_ShouldBeCorrectlyParsed_WhenEmpty()
    {
        await Session.Http
            .GetOptionInQuery(None)
            .Send()
            .Verify();
    }

    [Fact]
    public async Task OptionInQuery_ShouldBeCorrectlyParsed_WhenSome()
    {
        await Session.Http
            .GetOptionInQuery(Some("Hello World!"))
            .Send()
            .Verify();
    }
}
