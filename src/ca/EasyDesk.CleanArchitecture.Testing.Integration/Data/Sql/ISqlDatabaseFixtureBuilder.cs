namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public interface ISqlDatabaseFixtureBuilder
{
    ISqlDatabaseFixtureBuilder WithTableCopies();

    ISqlDatabaseFixtureBuilder OverrideConnectionStringFromConfiguration(string configurationKey);

    ISqlDatabaseFixtureBuilder ModifyConnectionString(Func<string, string> update);
}
