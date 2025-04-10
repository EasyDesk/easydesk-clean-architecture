using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public class SqlDatabaseFixtureOptions
{
    private Func<string, string> _connectionStringModifier = It;
    private readonly TestFixtureConfigurer _configurer;
    private Lazy<string> _connectionString;

    public SqlDatabaseFixtureOptions(TestFixtureConfigurer configurer, Func<string> connectionString)
    {
        _configurer = configurer;
        _connectionString = new(() => _connectionStringModifier(connectionString()));
    }

    public SqlDatabaseFixtureOptions OverrideConnectionStringFromConfiguration(string configurationKey)
    {
        _configurer.ConfigureHost(host => host.WithConfiguration(configurationKey, GetActualConnectionString()));
        return this;
    }

    public SqlDatabaseFixtureOptions ModifyConnectionString(Func<string, string> update)
    {
        var currentModifier = _connectionStringModifier;
        _connectionStringModifier = c => update(currentModifier(c));
        return this;
    }

    public string GetActualConnectionString() => _connectionString.Value;
}
