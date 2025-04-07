using EasyDesk.Commons.Collections;
using System.Data;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.SqlServer;

internal class SqlServerTableCopiesProvider : ITableCopiesProvider
{
    public string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput) => $"""
        SELECT T.TABLE_SCHEMA AS [{schemaOutput}], T.TABLE_NAME AS [{tableOutput}], C.COLUMN_NAME AS [{columnOutput}]
        FROM INFORMATION_SCHEMA.TABLES T
        INNER JOIN INFORMATION_SCHEMA.COLUMNS C
        ON T.TABLE_SCHEMA = C.TABLE_SCHEMA AND T.TABLE_NAME = C.TABLE_NAME
        WHERE T.TABLE_TYPE = 'BASE TABLE';
        """;

    public string GenerateCopyTableCommand(TableDef table) => $"""
        SELECT *
        INTO {FormatCopyTable(table)}
        FROM {FormatTable(table)};
        """;

    public string GenerateDisableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE {FormatTable(table)} NOCHECK CONSTRAINT ALL;
        """;

    public string GenerateEnableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE {FormatTable(table)} CHECK CONSTRAINT ALL;
        """;

    public string GenerateRestoreTableCommand(TableDef table)
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
