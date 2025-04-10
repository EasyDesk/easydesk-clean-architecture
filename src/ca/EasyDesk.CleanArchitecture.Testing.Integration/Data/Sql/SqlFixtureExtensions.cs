using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using System.Data.Common;
using IDockerContainer = DotNet.Testcontainers.Containers.IContainer;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public static class SqlFixtureExtensions
{
    internal static TestFixtureConfigurer AddSqlDatabase<T>(
        this TestFixtureConfigurer configurer,
        T container,
        Func<T, string> getConnectionString,
        Func<string, DbConnection> createConnection,
        ITableCopiesProvider? tableCopiesProvider = null,
        Action<SqlDatabaseFixtureOptions>? configureOptions = null)
        where T : IDockerContainer
    {
        configurer.RegisterDockerContainer(container);

        var options = new SqlDatabaseFixtureOptions(configurer, () => getConnectionString(container));
        configureOptions?.Invoke(options);

        if (tableCopiesProvider is not null)
        {
            configurer.ContainerBuilder
                .Register(c => new TableCopiesManager(() => createConnection(options.GetActualConnectionString()), tableCopiesProvider))
                .SingleInstance();
        }

        return configurer;
    }

    public static TestFixtureConfigurer AddDatabaseResetUsingTableCopies(this TestFixtureConfigurer configurer)
    {
        return configurer.RegisterLifetimeHooks<DatabaseResetWithTableCopiesLifetimeHooks>();
    }

    private class DatabaseResetWithTableCopiesLifetimeHooks : LifetimeHooks
    {
        private readonly IEnumerable<TableCopiesManager> _tableCopiesManagers;

        public DatabaseResetWithTableCopiesLifetimeHooks(IEnumerable<TableCopiesManager> tableCopiesManagers)
        {
            _tableCopiesManagers = tableCopiesManagers;
        }

        public override async Task OnInitialization()
        {
            foreach (var manager in _tableCopiesManagers)
            {
                await manager.PrepareTableCopies();
            }
        }

        public override async Task BetweenTests()
        {
            foreach (var manager in _tableCopiesManagers)
            {
                await manager.RestoreDataFromTableCopies();
            }
        }
    }
}
