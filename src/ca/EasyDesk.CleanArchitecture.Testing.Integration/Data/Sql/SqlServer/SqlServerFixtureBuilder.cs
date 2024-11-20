using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using EasyDesk.Commons.Collections;
using Microsoft.Data.SqlClient;
using System.Data;
using Testcontainers.MsSql;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

internal class SqlServerFixtureBuilder<TFixture> : AbstractSqlFixtureBuilder<TFixture, MsSqlContainer>
    where TFixture : ITestFixture
{
    private readonly string _databaseName;

    public SqlServerFixtureBuilder(WebServiceTestsFixtureBuilder<TFixture> builder, MsSqlContainer container, string databaseName)
        : base(builder, container)
    {
        _databaseName = databaseName;
    }

    protected override string GetConnectionString()
    {
        var originalConnectionString = Container.GetConnectionString();
        var builder = new SqlConnectionStringBuilder(originalConnectionString)
        {
            InitialCatalog = _databaseName,
            TrustServerCertificate = true,
        };
        return builder.ConnectionString;
    }

    protected override string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput) => $"""
        SELECT T.TABLE_SCHEMA AS [{schemaOutput}], T.TABLE_NAME AS [{tableOutput}], C.COLUMN_NAME AS [{columnOutput}]
        FROM INFORMATION_SCHEMA.TABLES T
        INNER JOIN INFORMATION_SCHEMA.COLUMNS C
        ON T.TABLE_SCHEMA = C.TABLE_SCHEMA AND T.TABLE_NAME = C.TABLE_NAME
        WHERE T.TABLE_TYPE = 'BASE TABLE';
        """;

    protected override string GenerateCopyTableCommand(TableDef table) => $"""
        SELECT *
        INTO {FormatCopyTable(table)}
        FROM {FormatTable(table)};
        """;

    protected override string GenerateDisableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE {FormatTable(table)} NOCHECK CONSTRAINT ALL;
        """;

    protected override string GenerateEnableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE {FormatTable(table)} CHECK CONSTRAINT ALL;
        """;

    protected override string GenerateRestoreTableCommand(TableDef table)
    {
        var columnsList = table.Columns.Select(x => $"[{x}]").ConcatStrings(", ");
        return $"""
            DELETE FROM {FormatTable(table)};

            IF (OBJECTPROPERTY(OBJECT_ID('{table.Schema}.{table.Name}'), 'TableHasIdentity') = 1)
            BEGIN
                SET IDENTITY_INSERT {FormatTable(table)} ON;
            END

            INSERT INTO {FormatTable(table)} ({columnsList})
            SELECT {columnsList}
            FROM {FormatCopyTable(table)};

            IF (OBJECTPROPERTY(OBJECT_ID('{table.Schema}.{table.Name}'), 'TableHasIdentity') = 1)
            BEGIN
                SET IDENTITY_INSERT {FormatTable(table)} OFF;
            END
            """;
    }

    private string FormatCopyTable(TableDef table) => FormatTableRaw(table.Schema, $"__copy_{table.Name}__");

    private string FormatTable(TableDef table) => FormatTableRaw(table.Schema, table.Name);

    private string FormatTableRaw(string schema, string name) => $"[{schema}].[{name}]";
}
