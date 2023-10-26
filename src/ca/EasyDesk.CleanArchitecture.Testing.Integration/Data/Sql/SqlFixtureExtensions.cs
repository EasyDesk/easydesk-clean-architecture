using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public static class SqlFixtureExtensions
{
    internal static ISqlDatabaseFixtureBuilder AddSqlDatabase<TFixture, TContainer>(
        this WebServiceTestsFixtureBuilder<TFixture> builder,
        TContainer container,
        Func<WebServiceTestsFixtureBuilder<TFixture>, TContainer, ISqlDatabaseFixtureBuilder> sqlFixtureBuilder)
        where TFixture : WebServiceTestsFixture<TFixture>
        where TContainer : IContainer
    {
        builder.ConfigureContainers(c => c.RegisterTestContainer(container));
        return sqlFixtureBuilder(builder, container);
    }
}
