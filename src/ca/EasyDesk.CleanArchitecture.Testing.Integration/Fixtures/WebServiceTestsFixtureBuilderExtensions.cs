namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public static class WebServiceTestsFixtureBuilderExtensions
{
    public static WebServiceTestsFixtureBuilder<T> WithConfiguration<T>(
        this WebServiceTestsFixtureBuilder<T> builder,
        Action<IDictionary<string, string?>> configure)
        where T : ITestFixture
    {
        return builder.ConfigureWebService(s => s.WithConfiguration(configure));
    }

    public static WebServiceTestsFixtureBuilder<T> WithConfiguration<T>(
        this WebServiceTestsFixtureBuilder<T> builder,
        string key,
        string value)
        where T : ITestFixture
    {
        return builder.WithConfiguration(d => d.Add(key, value));
    }
}
