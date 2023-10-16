using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public static class SqlFixtureExtensions
{
    internal static ISqlDatabaseFixtureBuilder AddSqlDatabase<T>(
        this WebServiceTestsFixtureBuilder builder,
        T container,
        Func<WebServiceTestsFixtureBuilder, T, ISqlDatabaseFixtureBuilder> sqlFixtureBuilder)
        where T : IContainer
    {
        builder.ConfigureContainers(c => c.RegisterTestContainer(container));
        return sqlFixtureBuilder(builder, container);
    }
}
