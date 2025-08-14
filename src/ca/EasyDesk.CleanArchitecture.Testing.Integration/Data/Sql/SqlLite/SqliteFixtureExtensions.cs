using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using Microsoft.Data.Sqlite;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Sqlite;

public static class SqliteFixtureExtensions
{
    public static TestFixtureConfigurer AddSqliteDatabase(
        this TestFixtureConfigurer configurer,
        string configurationKey,
        string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        var connectionString = $"Data Source={filePath};";
        configurer.ContainerBuilder
            .Register(c => new TableCopiesManager(() => new SqliteConnection(connectionString), new SqliteTableCopiesProvider()))
            .SingleInstance();

        configurer.ConfigureHost(host => host.WithConfiguration(configurationKey, connectionString));

        return configurer;
    }
}
