namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public interface ISqlDatabaseFixtureBuilder
{
    ISqlDatabaseFixtureBuilder WithBackups();

    ISqlDatabaseFixtureBuilder WithRespawn(Action<RespawnerOptionsBuilder> options);

    ISqlDatabaseFixtureBuilder OverrideConnectionStringFromConfiguration(string configurationKey);

    ISqlDatabaseFixtureBuilder ModifyConnectionString(Func<string, string> update);
}
