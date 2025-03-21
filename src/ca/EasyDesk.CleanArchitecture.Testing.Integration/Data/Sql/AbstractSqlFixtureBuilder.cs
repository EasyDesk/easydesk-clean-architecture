﻿using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Commands;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

internal abstract class AbstractSqlFixtureBuilder<TFixture, TContainer> : ISqlDatabaseFixtureBuilder
    where TFixture : ITestFixture
    where TContainer : IContainer
{
    private const string SchemaOutput = "SCHEMA";
    private const string TableOutput = "TABLE";
    private const string ColumnOutput = "COLUMN";

    private Func<string, string> _connectionStringModifier = It;

    public AbstractSqlFixtureBuilder(WebServiceTestsFixtureBuilder<TFixture> builder, TContainer container)
    {
        Builder = builder;
        Container = container;
    }

    protected WebServiceTestsFixtureBuilder<TFixture> Builder { get; }

    protected TContainer Container { get; }

    protected record TableDef(string Schema, string Name, IFixedList<string> Columns);

    public ISqlDatabaseFixtureBuilder WithTableCopies()
    {
        string? restoreTablesCommand = null;
        Builder
            .OnInitialization(_ => UsingDbConnection(async connection =>
            {
                var tablesQuery = GenerateTablesQuery(SchemaOutput, TableOutput, ColumnOutput);

                var result = await connection
                    .RunQuery(tablesQuery, x => new
                    {
                        Schema = x.GetString(SchemaOutput),
                        TableName = x.GetString(TableOutput),
                        ColumnName = x.GetString(ColumnOutput),
                    })
                    .ToEnumerableAsync();

                var tables = result.GroupBy(
                    x => (x.Schema, x.TableName),
                    (k, xs) => new TableDef(k.Schema, k.TableName, xs.Select(c => c.ColumnName).ToFixedList()));

                var copyTablesCommand = tables.Select(GenerateCopyTableCommand).ConcatStrings("\n");
                await connection.RunCommand(copyTablesCommand);

                restoreTablesCommand = tables
                    .Select(GenerateDisableConstraintsCommand)
                    .Concat(tables.Select(GenerateRestoreTableCommand))
                    .Concat(tables.Select(GenerateEnableConstraintsCommand))
                    .ConcatStrings("\n");
            }))
            .OnReset(_ => UsingDbConnection(async connection =>
            {
                await connection.RunCommand(restoreTablesCommand!);
            }));

        return this;
    }

    protected abstract string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput);

    protected abstract string GenerateCopyTableCommand(TableDef table);

    protected abstract string GenerateDisableConstraintsCommand(TableDef table);

    protected abstract string GenerateEnableConstraintsCommand(TableDef table);

    protected abstract string GenerateRestoreTableCommand(TableDef table);

    private async Task UsingDbConnection(AsyncAction<DbConnection> action)
    {
        await using var connection = CreateConnection(GetActualConnectionString());
        await connection.OpenAsync();
        await action(connection);
    }

    protected virtual DbConnection CreateConnection(string connectionString) => new SqlConnection(connectionString);

    public ISqlDatabaseFixtureBuilder ModifyConnectionString(Func<string, string> update)
    {
        var currentModifier = _connectionStringModifier;
        _connectionStringModifier = c => update(currentModifier(c));
        return this;
    }

    public ISqlDatabaseFixtureBuilder OverrideConnectionStringFromConfiguration(string configurationKey)
    {
        Builder.WithConfiguration(config =>
        {
            var connectionString = GetActualConnectionString();
            config[configurationKey] = connectionString;
        });
        return this;
    }

    private string GetActualConnectionString() => _connectionStringModifier(GetConnectionString());

    protected abstract string GetConnectionString();
}
