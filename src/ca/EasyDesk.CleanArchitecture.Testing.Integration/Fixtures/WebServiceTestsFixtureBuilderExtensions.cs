namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public static class WebServiceTestsFixtureBuilderExtensions
{
    public static WebServiceTestsFixtureBuilder WithConfiguration(
        this WebServiceTestsFixtureBuilder builder,
        Action<IDictionary<string, string?>> configure)
    {
        return builder.ConfigureWebService(s => s.WithConfiguration(configure));
    }

    public static WebServiceTestsFixtureBuilder WithConfiguration(
        this WebServiceTestsFixtureBuilder builder,
        string key,
        string value)
    {
        return builder.WithConfiguration(d => d.Add(key, value));
    }
}
